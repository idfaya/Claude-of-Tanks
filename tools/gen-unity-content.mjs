import { mkdir, readFile, writeFile } from 'node:fs/promises';
import '../src/vehicles/tankFactory.ts';
import {
  ALL_TANK_IDS,
  PRODUCTION_TANK_IDS,
  SAVED_TANK_IDS,
  TANK_SPECS,
} from '../src/vehicles/specs.ts';
import { MAP_IDS, getMapConfig } from '../src/world/maps/index.ts';
import { createLayout } from '../src/world/terrain.ts';

const outputUrl = new URL('../Assets/ClaudeOfTanks/Resources/Generated/content-catalog.json', import.meta.url);
const check = process.argv.includes('--check');

function canonical(value) {
  if (Array.isArray(value)) return value.map(canonical);
  if (!value || typeof value !== 'object') return value;
  return Object.fromEntries(Object.keys(value).sort().flatMap((key) => {
    const child = value[key];
    return typeof child === 'function' || child === undefined ? [] : [[key, canonical(child)]];
  }));
}

function vehicleRecord(spec) {
  const armor = spec.armor || {};
  const point = (v) => ({ x: v?.[0] || 0, y: v?.[1] || 0, z: v?.[2] || 0 });
  const plate = (value) => ({
    name: value.name, kind: value.kind, physicalMm: value.physicalMm,
    keMm: value.keMm, ceMm: value.ceMm,
    verts: (value.verts || []).map(point),
  });
  return {
    id: spec.id, name: spec.name, nation: spec.nation, era: spec.era, role: spec.role,
    variantOf: spec.variantOf, hp: spec.hp, enginePowerHp: spec.enginePowerHp,
    weightTons: spec.weightTons, topSpeedKmh: spec.topSpeedKmh,
    reverseSpeedKmh: spec.reverseSpeedKmh, hullTraverseDegS: spec.hullTraverseDegS,
    terrainResistance: spec.terrainResistance, trackTraction: spec.trackTraction,
    pivotStyle: spec.pivotStyle, turretTraverseDegS: spec.turretTraverseDegS,
    gunPitchDegS: spec.gunPitchDegS, gunElevationDeg: spec.gunElevationDeg,
    gunDepressionDeg: spec.gunDepressionDeg, gunArcDeg: spec.gunArcDeg,
    hydropneumaticAim: spec.hydropneumaticAim, gun: spec.gun, dims: spec.dims,
    armor: {
      boundingRadiusM: armor.boundingRadiusM, turretless: armor.turretless,
      turretPivot: point(armor.turretPivot), gunPivot: point(armor.gunPivot),
      gunBarrel: armor.gunBarrel,
      hullPlates: (armor.hullPlates || []).map(plate),
      turretPlates: (armor.turretPlates || []).map(plate),
      modules: armor.modules, crew: armor.crew, trackShapes: armor.trackShapes,
      bodyContactPoints: armor.bodyContactPoints,
    },
    visual: spec.visual, roster: spec.roster,
  };
}

function cssColor(value, fallback) {
  const text = String(value || '');
  const rgba = text.match(/rgba?\(\s*([\d.]+)\s*,\s*([\d.]+)\s*,\s*([\d.]+)/i);
  if (rgba) {
    return {
      r: Number(rgba[1]) / 255,
      g: Number(rgba[2]) / 255,
      b: Number(rgba[3]) / 255,
    };
  }
  if (/^#[0-9a-f]{6}$/i.test(text)) {
    const number = Number.parseInt(text.slice(1), 16);
    return {
      r: ((number >> 16) & 255) / 255,
      g: ((number >> 8) & 255) / 255,
      b: (number & 255) / 255,
    };
  }
  return fallback;
}

function rgb(value, fallback) {
  return Array.isArray(value) && value.length >= 3
    ? { r: value[0] / 255, g: value[1] / 255, b: value[2] / 255 }
    : fallback;
}

function mapRecord(id) {
  const config = getMapConfig(id);
  const layout = createLayout(config);
  const minimap = config.minimap || {};
  const fallbackGround = { r: 0.28, g: 0.34, b: 0.22 };
  return {
    ...config,
    unitySurface: {
      roads: layout.roads.map((line) => ({
        points: line.map(([x, z]) => ({ x, z })),
      })),
      lakes: layout.lakes.map((disc) => ({
        x: disc.x, z: disc.z, r: disc.r,
        depth: disc.depth || 0, level: disc.level || 0,
      })),
      marshes: layout.marshes.map((disc) => ({
        x: disc.x, z: disc.z, r: disc.r,
        depth: disc.dip || disc.depth || 0, level: disc.level || 0,
      })),
      frozenWater: layout.terrain.frozenMarshes === true,
      groundColor: rgb(minimap.base, fallbackGround),
      hardColor: rgb(minimap.hard, { r: 0.42, g: 0.4, b: 0.34 }),
      softColor: rgb(minimap.soft, { r: 0.2, g: 0.28, b: 0.22 }),
      roadColor: cssColor(minimap.roadFill, { r: 0.62, g: 0.56, b: 0.44 }),
      roadCasingColor: cssColor(minimap.roadCasing, { r: 0.2, g: 0.18, b: 0.14 }),
      waterColor: cssColor(minimap.water, { r: 0.2, g: 0.36, b: 0.4 }),
    },
  };
}

const payload = canonical({
  schemaVersion: 2,
  counts: {
    savedVehicles: SAVED_TANK_IDS.length,
    releaseVehicles: ALL_TANK_IDS.length,
    productionVehicles: PRODUCTION_TANK_IDS.length,
    maps: MAP_IDS.length,
  },
  catalogs: {
    saved: SAVED_TANK_IDS,
    release: ALL_TANK_IDS,
    production: PRODUCTION_TANK_IDS,
  },
  vehicles: SAVED_TANK_IDS.map((id) => vehicleRecord(TANK_SPECS[id])),
  maps: MAP_IDS.map(mapRecord),
});
const output = `${JSON.stringify(payload, null, 2)}\n`;

if (check) {
  const current = await readFile(outputUrl, 'utf8').catch(() => '');
  if (current !== output) {
    console.error('Unity content catalog is stale; run npm run unity:content:update');
    process.exitCode = 1;
  } else {
    console.log(`unity-content: ${SAVED_TANK_IDS.length} vehicles and ${MAP_IDS.length} maps current`);
  }
} else {
  await mkdir(new URL('.', outputUrl), { recursive: true });
  await writeFile(outputUrl, output, 'utf8');
  console.log(`unity-content: wrote ${SAVED_TANK_IDS.length} vehicles and ${MAP_IDS.length} maps`);
}
