import assert from 'node:assert/strict';
import {
  readFileSync,
  statSync,
} from 'node:fs';
import { gunzipSync } from 'node:zlib';
import { resolve } from 'node:path';

const root = process.cwd();
const geometryRoot = resolve(
  root,
  'Assets/ClaudeOfTanks/Resources/Content/VehicleGeometry',
);
const catalog = JSON.parse(
  readFileSync(
    resolve(
      root,
      'Assets/ClaudeOfTanks/Resources/Content/content-catalog.json',
    ),
    'utf8',
  ),
);
const manifest = JSON.parse(
  readFileSync(
    resolve(geometryRoot, 'manifest.json'),
    'utf8',
  ),
);

assert.equal(manifest.schemaVersion, 1);
assert.equal(manifest.vehicles.length, catalog.vehicles.length);
assert.deepEqual(
  manifest.vehicles.map((vehicle) => vehicle.id),
  catalog.vehicles.map((vehicle) => vehicle.id),
  'geometry recipes must follow the canonical catalog order',
);

for (const vehicle of manifest.vehicles) {
  const path = resolve(geometryRoot, `${vehicle.id}.bytes`);
  const compressed = readFileSync(path);
  assert.equal(
    statSync(path).size,
    vehicle.compressedBytes,
    `${vehicle.id}: compressed size receipt`,
  );
  const payload = gunzipSync(compressed);
  assert.equal(
    payload.subarray(0, 4).toString('ascii'),
    'CTG3',
    `${vehicle.id}: recipe magic`,
  );
  assert.equal(
    payload.readUInt16LE(4),
    3,
    `${vehicle.id}: recipe version`,
  );
  assert.equal(
    payload.readUInt16LE(6),
    vehicle.sourceCount,
    `${vehicle.id}: source count`,
  );
  const idLength = payload.readUInt16LE(8);
  assert.equal(
    payload.subarray(10, 10 + idLength).toString('utf8'),
    vehicle.id,
    `${vehicle.id}: embedded id`,
  );
}

console.log(
  `unity-geometry.selftest: ${manifest.vehicles.length} ` +
  'compressed TS geometry recipes are structurally valid',
);
