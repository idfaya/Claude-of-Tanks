import assert from 'node:assert/strict';
import { execFile, spawn } from 'node:child_process';
import { rm } from 'node:fs/promises';
import net from 'node:net';
import path from 'node:path';
import { promisify } from 'node:util';
import WebSocket from 'ws';

const execFileAsync = promisify(execFile);
const root = path.resolve(new URL('..', import.meta.url).pathname);
const dotnet = path.join(root, 'tools', 'dotnet.sh');
const project = path.join(
  root,
  'server',
  'dotnet',
  'ClaudeOfTanks.Server',
  'ClaudeOfTanks.Server.csproj',
);
const assembly = path.join(
  root,
  'server',
  'dotnet',
  'ClaudeOfTanks.Server',
  'bin',
  'Release',
  'net8.0',
  'ClaudeOfTanks.Server.dll',
);
const scratch = path.join(root, '.qa-device', 'dotnet-server-smoke', String(process.pid));
const origin = 'http://127.0.0.1';

const delay = (milliseconds) => new Promise((resolve) => {
  setTimeout(resolve, milliseconds);
});

const waitForExit = (child, milliseconds) => {
  if (child.exitCode !== null || child.signalCode !== null) {
    return Promise.resolve(true);
  }
  return Promise.race([
    new Promise((resolve) => child.once('exit', () => resolve(true))),
    delay(milliseconds).then(() => false),
  ]);
};

const signalChild = (child, signal) => {
  if (child.exitCode !== null || child.signalCode !== null) return;
  try {
    process.kill(-child.pid, signal);
  } catch {
    child.kill(signal);
  }
};

const stopChild = async (child) => {
  if (!(await waitForExit(child, 1_000))) {
    signalChild(child, 'SIGTERM');
    if (!(await waitForExit(child, 2_000))) {
      signalChild(child, 'SIGKILL');
      await waitForExit(child, 2_000);
    }
  }
  child.stdout?.destroy();
  child.stderr?.destroy();
};

const reservePort = () => new Promise((resolve, reject) => {
  const server = net.createServer();
  server.once('error', reject);
  server.listen(0, '127.0.0.1', () => {
    const { port } = server.address();
    server.close((error) => error ? reject(error) : resolve(port));
  });
});

const waitForHealth = async (url, child) => {
  const deadline = Date.now() + 15_000;
  while (Date.now() < deadline) {
    if (child.exitCode !== null || child.signalCode !== null) {
      throw new Error(
        `Standalone server exited early with ${child.exitCode ?? child.signalCode}`,
      );
    }
    try {
      const response = await fetch(`${url}/healthz`);
      if (response.ok) return response.json();
    } catch {
      // The process can be compiling or binding its listeners.
    }
    await new Promise((resolve) => setTimeout(resolve, 50));
  }
  throw new Error('Standalone server health check timed out');
};

const requestJson = async (url, options = {}) => {
  const response = await fetch(url, options);
  const body = await response.json();
  assert.ok(response.ok, `${response.status}: ${JSON.stringify(body)}`);
  return body;
};

const createIdentity = (baseUrl, name) => requestJson(
  `${baseUrl}/ranked/identity`,
  {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ name }),
  },
);

const joinQueue = (baseUrl, identity, specId) => requestJson(
  `${baseUrl}/ranked/queue`,
  {
    method: 'POST',
    headers: {
      authorization: `Bearer ${identity.token}`,
      'content-type': 'application/json',
    },
    body: JSON.stringify({
      playerId: identity.playerId,
      specId,
      equipment: [],
      camo: 'factory',
      teamSize: 1,
    }),
  },
);

const pollQueue = (baseUrl, queue) => requestJson(
  `${baseUrl}/ranked/queue/${queue.ticketId}`,
  { headers: { authorization: `Bearer ${queue.ticketToken}` } },
);

const openSocket = (url) => new Promise((resolve, reject) => {
  const socket = new WebSocket(url, { origin });
  socket.once('open', () => resolve(socket));
  socket.once('error', reject);
});

const nextMessage = (socket) => new Promise((resolve, reject) => {
  socket.once('message', (data) => resolve(data));
  socket.once('error', reject);
  socket.once('close', () => reject(
    new Error('WebSocket closed before the expected message'),
  ));
});

const closeSocket = async (socket) => {
  if (
    socket.readyState === WebSocket.CLOSED ||
    socket.readyState === WebSocket.CLOSING
  ) {
    return;
  }
  const closed = new Promise((resolve) => {
    socket.once('close', resolve);
    socket.once('error', resolve);
  });
  socket.close();
  await Promise.race([closed, delay(1_000)]);
  if (socket.readyState !== WebSocket.CLOSED) {
    socket.terminate();
    await Promise.race([closed, delay(1_000)]);
  }
};

const writeString = (value) => {
  const bytes = Buffer.from(value, 'utf8');
  assert.ok(bytes.length > 0 && bytes.length <= 256);
  const result = Buffer.allocUnsafe(2 + bytes.length);
  result.writeUInt16LE(bytes.length, 0);
  bytes.copy(result, 2);
  return result;
};

const writeByteString = (value) => {
  const bytes = Buffer.from(value, 'utf8');
  assert.ok(bytes.length > 0 && bytes.length <= 64);
  return Buffer.concat([Buffer.from([bytes.length]), bytes]);
};

const encodeInput = ({
  playerId,
  sequence,
  actionSequence = 0,
  throttle = 0,
  actions = 0,
}) => {
  const header = Buffer.allocUnsafe(64);
  let offset = 0;
  header.writeUInt32LE(0x49544f43, offset); offset += 4;
  header.writeUInt16LE(3, offset); offset += 2;
  const player = writeByteString(playerId);
  header.writeUInt32LE(sequence, offset); offset += 4;
  header.writeUInt32LE(actionSequence, offset); offset += 4;
  header.writeBigInt64LE(BigInt(sequence), offset); offset += 8;
  header.writeBigInt64LE(-1n, offset); offset += 8;
  header.writeFloatLE(throttle, offset); offset += 4;
  header.writeFloatLE(0, offset); offset += 4;
  header.writeUInt8(0, offset); offset += 1;
  header.writeFloatLE(0, offset); offset += 4;
  header.writeFloatLE(0, offset); offset += 4;
  header.writeFloatLE(100, offset); offset += 4;
  header.writeUInt8(0, offset); offset += 1;
  header.writeUInt8(actions, offset); offset += 1;
  return Buffer.concat([header.subarray(0, 6), player, header.subarray(6, offset)]);
};

const encodeLane = (lane, payload) => Buffer.concat([
  Buffer.from([0x43, 0x4f, 0x54, 1, lane]),
  payload,
]);

const encodeTicket = (ticket) => {
  const header = Buffer.allocUnsafe(7);
  header.writeUInt32LE(0x41544f43, 0);
  header.writeUInt16LE(1, 4);
  header.writeUInt8(1, 6);
  return Buffer.concat([
    header,
    writeString(ticket.matchId),
    writeString(ticket.playerId),
    writeString(ticket.token),
  ]);
};

const readString = (buffer, state) => {
  const length = buffer.readUInt16LE(state.offset);
  state.offset += 2;
  const value = buffer.toString('utf8', state.offset, state.offset + length);
  state.offset += length;
  return value;
};

const decodeAdmission = (data) => {
  const buffer = Buffer.from(data);
  const state = { offset: 0 };
  assert.equal(buffer.readUInt32LE(state.offset), 0x52544f43);
  state.offset += 4;
  assert.equal(buffer.readUInt16LE(state.offset), 1);
  state.offset += 2;
  const response = {
    matchId: readString(buffer, state),
    playerId: readString(buffer, state),
    entityId: readString(buffer, state),
    sessionToken: readString(buffer, state),
  };
  response.connectionGeneration = buffer.readInt32LE(state.offset);
  state.offset += 4;
  assert.equal(state.offset, buffer.length);
  return response;
};

const decodeSnapshotFrame = (data) => {
  const laneFrame = Buffer.from(data);
  assert.deepEqual(
    [...laneFrame.subarray(0, 5)],
    [0x43, 0x4f, 0x54, 1, 2],
  );
  const buffer = laneFrame.subarray(5);
  let offset = 0;
  assert.equal(buffer.readUInt32LE(offset), 0x44544f43); offset += 4;
  assert.equal(buffer.readUInt16LE(offset), 3); offset += 2;
  const baseTick = Number(buffer.readBigInt64LE(offset)); offset += 8;
  const removed = buffer.readUInt16LE(offset); offset += 2;
  for (let i = 0; i < removed; i += 1) {
    const length = buffer.readUInt8(offset); offset += 1 + length;
  }
  const payloadLength = buffer.readInt32LE(offset); offset += 4;
  assert.equal(payloadLength, buffer.length - offset);
  assert.equal(buffer.readUInt32LE(offset), 0x4e544f43); offset += 4;
  assert.equal(buffer.readUInt16LE(offset), 7); offset += 2;
  const tick = Number(buffer.readBigInt64LE(offset));
  return { baseTick, tick };
};

const verifySignaling = async (port) => {
  const socket = await openSocket(`ws://127.0.0.1:${port}/signal`);
  socket.send(JSON.stringify({
    type: 'room_create',
    requestId: 'dotnet-smoke',
    payload: {
      mode: 'private',
      maxPlayers: 4,
      sessionId: 'dotnet-smoke-session',
      player: { id: 'dotnet-smoke-host', name: 'Host' },
    },
  }));
  const response = JSON.parse((await nextMessage(socket)).toString());
  assert.equal(response.type, 'room_created');
  assert.equal(response.requestId, 'dotnet-smoke');
  assert.match(response.payload.roomCode, /^[A-Z0-9]{6}$/);
  assert.ok(response.payload.resumeToken.length >= 32);
  await closeSocket(socket);
};

await execFileAsync(dotnet, [
  'build',
  project,
  '--configuration',
  'Release',
  '--nologo',
], { cwd: root });

const [matchPort, signalPort] = await Promise.all([reservePort(), reservePort()]);
const child = spawn(dotnet, [
  assembly,
  '--cot-bind=127.0.0.1',
  `--cot-port=${matchPort}`,
  `--cot-signal-port=${signalPort}`,
  `--cot-origins=${origin}`,
  `--cot-rating-file=${path.join(scratch, 'ratings.bin')}`,
  '--cot-run-for-ms=15000',
], {
  cwd: root,
  env: { ...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: '1' },
  detached: true,
  stdio: ['ignore', 'pipe', 'pipe'],
});
let output = '';
child.stdout.on('data', (data) => { output += data; });
child.stderr.on('data', (data) => { output += data; });

try {
  const baseUrl = `http://127.0.0.1:${matchPort}`;
  const health = await waitForHealth(baseUrl, child);
  assert.deepEqual(
    { ok: health.ok, service: health.service },
    { ok: true, service: 'cot-match' },
  );

  const [alpha, bravo] = await Promise.all([
    createIdentity(baseUrl, 'Alpha'),
    createIdentity(baseUrl, 'Bravo'),
  ]);
  const alphaQueue = await joinQueue(baseUrl, alpha, 'm1a1');
  const bravoQueue = await joinQueue(baseUrl, bravo, 't90m');
  const [alphaMatch, bravoMatch] = await Promise.all([
    pollQueue(baseUrl, alphaQueue),
    pollQueue(baseUrl, bravoQueue),
  ]);
  assert.equal(alphaMatch.status, 'matched');
  assert.equal(bravoMatch.status, 'matched');
  assert.equal(alphaMatch.match.matchId, bravoMatch.match.matchId);

  const [alphaSocket, bravoSocket] = await Promise.all([
    openSocket(`ws://127.0.0.1:${matchPort}/match`),
    openSocket(`ws://127.0.0.1:${matchPort}/match`),
  ]);
  alphaSocket.send(encodeTicket(alphaMatch.match));
  bravoSocket.send(encodeTicket(bravoMatch.match));
  const [admission, bravoAdmission] = await Promise.all([
    nextMessage(alphaSocket).then(decodeAdmission),
    nextMessage(bravoSocket).then(decodeAdmission),
  ]);
  assert.equal(admission.matchId, alphaMatch.match.matchId);
  assert.equal(admission.playerId, alpha.playerId);
  assert.equal(bravoAdmission.playerId, bravo.playerId);
  assert.ok(admission.connectionGeneration >= 1);

  alphaSocket.send(encodeLane(1, encodeInput({
    playerId: alpha.playerId,
    sequence: 1,
    throttle: 1,
  })));
  bravoSocket.send(encodeLane(1, encodeInput({
    playerId: bravo.playerId,
    sequence: 1,
  })));
  const [alphaFrame, bravoFrame] = await Promise.all([
    nextMessage(alphaSocket).then(decodeSnapshotFrame),
    nextMessage(bravoSocket).then(decodeSnapshotFrame),
  ]);
  assert.ok(alphaFrame.tick > 0);
  assert.equal(bravoFrame.tick, alphaFrame.tick);
  assert.equal(alphaFrame.baseTick, -1);
  await Promise.all([
    closeSocket(alphaSocket),
    closeSocket(bravoSocket),
  ]);

  await verifySignaling(signalPort);
  console.log(JSON.stringify({
    ok: true,
    health,
    matchId: admission.matchId,
    playerId: admission.playerId,
    authorityTick: alphaFrame.tick,
    signaling: true,
  }, null, 2));
} catch (error) {
  console.error(output);
  throw error;
} finally {
  await stopChild(child);
  await rm(scratch, { recursive: true, force: true });
}
