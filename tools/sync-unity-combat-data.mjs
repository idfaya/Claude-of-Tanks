import { readFile, writeFile } from 'node:fs/promises';
import '../src/vehicles/tankFactory.ts';
import { TANK_SPECS } from '../src/vehicles/specs.ts';

const catalogUrl = new URL(
  '../Assets/ClaudeOfTanks/Resources/Content/content-catalog.json',
  import.meta.url,
);
const check = process.argv.includes('--check');

function assignDefined(target, source, keys) {
  for (const key of keys) {
    if (source[key] !== undefined) target[key] = source[key];
  }
}

function scalarTerrainResistance(source) {
  const value = source.terrainResistance;
  if (typeof value === 'number') return value;
  if (value && typeof value.medium === 'number') return value.medium;
  return undefined;
}

function syncShells(targetGun, sourceGun, vehicleId) {
  const targetShells = targetGun.shells || [];
  const sourceShells = sourceGun.shells || [];
  if (targetShells.length !== sourceShells.length) {
    throw new Error(`${vehicleId}: shell count mismatch`);
  }
  targetGun.primaryGuided = sourceGun.primaryGuided === true;
  for (let index = 0; index < targetShells.length; index++) {
    assignDefined(targetShells[index], sourceShells[index], [
      'gravityScale',
      'guidanceTurnRateRadS',
      'moduleDmg',
      'effectiveOvermatchCaliberMm',
      'tandem',
      'soundProfile',
    ]);
  }
}

function syncPlates(targetPlates, sourcePlates, vehicleId, surface) {
  if (targetPlates.length !== sourcePlates.length) {
    throw new Error(`${vehicleId}: ${surface} plate count mismatch`);
  }
  for (let index = 0; index < targetPlates.length; index++) {
    const target = targetPlates[index];
    const source = sourcePlates[index];
    if (target.name !== source.name) {
      throw new Error(
        `${vehicleId}: ${surface} plate ${index} mismatch ` +
        `(${target.name} != ${source.name})`,
      );
    }
    if (source.era) target.era = source.era;
    if (source.moduleLink) target.moduleLink = source.moduleLink;
    if (source.gunFollow) target.gunFollow = true;
  }
}

function syncVehicle(target, source) {
  assignDefined(target, source, [
    'gunPitchDegS',
    'gunElevationDeg',
    'gunDepressionDeg',
    'gunArcDeg',
    'trackTraction',
  ]);
  const terrainResistance = scalarTerrainResistance(source);
  if (terrainResistance !== undefined) {
    target.terrainResistance = terrainResistance;
  }
  syncShells(target.gun, source.gun, target.id);
  syncPlates(
    target.armor.hullPlates,
    source.armor.hullPlates,
    target.id,
    'hull',
  );
  syncPlates(
    target.armor.turretPlates,
    source.armor.turretPlates,
    target.id,
    'turret',
  );
}

const before = await readFile(catalogUrl, 'utf8');
const catalog = JSON.parse(before);
for (const vehicle of catalog.vehicles) {
  const source = TANK_SPECS[vehicle.id];
  if (!source) throw new Error(`Missing TS vehicle spec: ${vehicle.id}`);
  syncVehicle(vehicle, source);
}

const after = `${JSON.stringify(catalog, null, 2)}\n`;
if (check) {
  if (after !== before) {
    throw new Error(
      'Unity combat catalog is stale. Run npm run unity:combat:update.',
    );
  }
} else {
  await writeFile(catalogUrl, after, 'utf8');
  console.log(`Synced ${catalog.vehicles.length} Unity vehicle combat records.`);
}
