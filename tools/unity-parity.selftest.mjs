import { spawnSync } from 'node:child_process';

const result = spawnSync(
  process.execPath,
  ['tools/generate-unity-parity-vectors.mjs', '--check'],
  {
    cwd: process.cwd(),
    env: process.env,
    stdio: 'inherit',
  },
);

if (result.error) throw result.error;
if (result.status !== 0) process.exit(result.status ?? 1);

console.log('unity-parity.selftest: TS/C# golden parity fixture is current');
