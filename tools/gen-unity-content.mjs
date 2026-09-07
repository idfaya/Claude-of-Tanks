import { mkdir, readFile, writeFile } from 'node:fs/promises';
import '../src/vehicles/tankFactory.ts';
import {
  ALL_TANK_IDS,
  PRODUCTION_TANK_IDS,
  SAVED_TANK_IDS,
  TANK_SPECS,
} from '../src/vehicles/specs.ts';
import { EQUIPMENT_CATALOG } from '../src/game/equipment.ts';
import {
  CAMO_CATALOG_PATTERN_IDS,
  CAMO_PATTERN_LABEL,
} from '../src/vehicles/camoPolicy.ts';
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

const TOWER_KINDS = new Set([
  'tower', 'church', 'chapel', 'onionchurch', 'minaret', 'lighthouse',
  'watertower', 'stack', 'needletower', 'broadcasttower', 'relaystation',
]);
const LARGE_KINDS = new Set([
  'factory', 'warehouse', 'depot', 'foundryoffice', 'firestation',
  'caravanserai', 'compound', 'compoundSouk', 'marketRow', 'motorpool',
  'servicegarage', 'parkingdeck', 'civichall', 'arcology', 'megatower',
  'terracetower', 'gantry',
]);
const INDUSTRIAL_KINDS = new Set([
  'factory', 'warehouse', 'depot', 'foundryoffice', 'firestation',
  'containerRow', 'gantry', 'watertower', 'stack', 'shed', 'motorpool',
  'quonsethut', 'transformershed', 'securityoffice', 'servicegarage',
  'relaystation', 'parkingdeck', 'broadcasttower',
]);
const RUIN_KINDS = new Set(['ruin']);

function stableHash(value) {
  let hash = 17;
  for (let i = 0; i < value.length; i++) hash = Math.imul(hash, 31) + value.charCodeAt(i) | 0;
  return hash >>> 0;
}

function mulberry32(seed) {
  return () => {
    seed |= 0;
    seed = seed + 0x6D2B79F5 | 0;
    let value = Math.imul(seed ^ seed >>> 15, 1 | seed);
    value = value + Math.imul(value ^ value >>> 7, 61 | value) ^ value;
    return ((value ^ value >>> 14) >>> 0) / 4294967296;
  };
}

function structureShape(kind) {
  if (kind === 'megatower' || kind === 'arcology') {
    return { w: 24, d: 22, h: 48, profile: 'tower' };
  }
  if (kind === 'terracetower' || kind === 'needletower') {
    return { w: 15, d: 15, h: 38, profile: 'tower' };
  }
  if (TOWER_KINDS.has(kind)) {
    const height = kind === 'lighthouse' || kind === 'broadcasttower' ? 28 : 20;
    return { w: 9, d: 9, h: height, profile: 'tower' };
  }
  if (LARGE_KINDS.has(kind)) {
    return {
      w: kind === 'parkingdeck' ? 26 : 18,
      d: kind === 'gantry' ? 9 : 15,
      h: kind === 'parkingdeck' ? 13 : 11,
      profile: 'industrial',
    };
  }
  if (kind === 'rowhouse' || kind === 'cornershop') {
    return { w: 9.5, d: 10, h: 10.5, profile: 'urban' };
  }
  if (RUIN_KINDS.has(kind)) return { w: 10, d: 9, h: 6, profile: 'ruin' };
  if (kind === 'deserttent' || kind === 'commandtent') {
    return { w: 7, d: 9, h: 4, profile: 'tent' };
  }
  if (kind === 'huntingblind' || kind === 'guardpost' || kind === 'checkpointhut') {
    return { w: 5, d: 5, h: 5, profile: 'post' };
  }
  return { w: 10, d: 12, h: 7.5, profile: INDUSTRIAL_KINDS.has(kind) ? 'industrial' : 'rural' };
}

function structureLayout(config, layout) {
  const props = config.props || {};
  const plan = props.plan || [];
  const village = layout.village;
  const latBase = props.buildingLat?.[0] ?? 10;
  const latSpread = props.buildingLat?.[1] ?? 4;
  const candidates = [];
  for (let roadIndex = 0; roadIndex < layout.roads.length; roadIndex++) {
    const road = layout.roads[roadIndex];
    for (let pointIndex = 1; pointIndex < road.length - 1; pointIndex++) {
      const [x, z] = road[pointIndex];
      if (x < village.x0 || x > village.x1 || z < village.z0 || z > village.z1) continue;
      const tx = road[pointIndex + 1][0] - road[pointIndex - 1][0];
      const tz = road[pointIndex + 1][1] - road[pointIndex - 1][1];
      const length = Math.hypot(tx, tz) || 1;
      for (const side of [-1, 1]) {
        candidates.push({ x, z, tx: tx / length, tz: tz / length, side, roadIndex, pointIndex });
      }
    }
  }
  if (!candidates.length) {
    candidates.push({ x: village.cx, z: village.cz, tx: 0, tz: 1, side: 1 });
  }
  const rng = mulberry32(stableHash(config.id + '-unity-structures'));
  for (let i = candidates.length - 1; i > 0; i--) {
    const swap = Math.floor(rng() * (i + 1));
    [candidates[i], candidates[swap]] = [candidates[swap], candidates[i]];
  }

  const buildings = plan.map((kind, index) => {
    const candidate = candidates[index % candidates.length];
    const shape = structureShape(kind);
    const ring = Math.floor(index / candidates.length);
    const lateral = latBase + (index % 5) / 4 * latSpread +
      shape.d * 0.5 + ring * (shape.d + 4);
    return {
      kind,
      profile: shape.profile,
      x: candidate.x - candidate.tz * candidate.side * lateral,
      z: candidate.z + candidate.tx * candidate.side * lateral,
      w: shape.w,
      d: shape.d,
      h: shape.h,
      yawDeg: Math.atan2(candidate.tx, candidate.tz) * 180 / Math.PI,
      tactical: false,
      destructible: false,
    };
  });
  for (const beat of props.tacticalBeats || []) {
    if (!beat.structure) continue;
    const shape = structureShape(beat.structure);
    buildings.push({
      kind: beat.structure,
      profile: shape.profile,
      x: beat.x,
      z: beat.z,
      w: shape.w,
      d: shape.d,
      h: shape.h,
      yawDeg: beat.yawDeg || 0,
      tactical: true,
      destructible: true,
    });
  }
  return {
    buildings,
    walls: (props.wallRuns || []).map(([x1, z1, x2, z2, variant = 0]) => ({
      x1, z1, x2, z2, variant,
    })),
    rubblePiles: props.rubblePiles || 0,
    sandbagLines: props.sandbagLines || 0,
    hedgehogs: props.hedgehogs || 0,
    buildingColor: cssColor(config.minimap?.buildingFill, { r: 0.66, g: 0.64, b: 0.6 }),
  };
}

function pickSpecies(mix, rng) {
  const choices = Array.isArray(mix) && mix.length ? mix : [['oak', 1]];
  const total = choices.reduce((sum, entry) => sum + Math.max(0, entry[1] || 0), 0) || 1;
  let roll = rng() * total;
  for (const [species, weight] of choices) {
    roll -= Math.max(0, weight || 0);
    if (roll <= 0) return species;
  }
  return choices[choices.length - 1][0];
}

function vegetationLayout(config, layout, structures) {
  const vegetation = config.vegetation || {};
  const rng = mulberry32(stableHash(config.id + '-unity-vegetation'));
  const stands = [];
  const lakes = layout.lakes || [];
  const avoid = vegetation.avoid || [];
  const buildings = structures.buildings || [];
  const spawns = [layout.spawns?.player, ...(layout.spawns?.enemies || [])].filter(Boolean);
  const clear = (x, z, padding = 0) => {
    if (Math.max(Math.abs(x), Math.abs(z)) > 490) return false;
    if (lakes.some((disc) => Math.hypot(x - disc.x, z - disc.z) < disc.r + padding)) return false;
    if (avoid.some((disc) => Math.hypot(x - disc.x, z - disc.z) < disc.r + padding)) return false;
    if (spawns.some((spawn) => Math.hypot(x - spawn.x, z - spawn.z) < 34 + padding)) return false;
    if (buildings.some((building) =>
      Math.abs(x - building.x) < building.w * 0.5 + 7 + padding &&
      Math.abs(z - building.z) < building.d * 0.5 + 7 + padding)) return false;
    return true;
  };
  const addStand = (zone, x, z, radius, count, species) => {
    stands.push({
      zone, x, z, radius, count, species,
      seed: stableHash(`${config.id}-${zone}-${stands.length}`),
    });
  };
  const placeRandom = (zone, count, extent, mix, radiusFor, treesFor) => {
    for (let index = 0; index < count; index++) {
      let x = 0, z = 0, placed = false;
      for (let attempt = 0; attempt < 300; attempt++) {
        x = (rng() * 2 - 1) * extent;
        z = (rng() * 2 - 1) * extent;
        if (clear(x, z, radiusFor(index) * 0.2)) {
          placed = true;
          break;
        }
      }
      if (!placed) {
        const angle = index * 2.399963229728653;
        const radius = 120 + (index % 17) * 17;
        x = Math.cos(angle) * radius;
        z = Math.sin(angle) * radius;
      }
      addStand(
        zone, x, z, radiusFor(index), treesFor(index),
        pickSpecies(mix, rng));
    }
  };

  placeRandom(
    'cluster',
    vegetation.clusterCount || 0,
    420,
    vegetation.clusterMix,
    () => 16 + rng() * 26,
    () => 24 + Math.floor(rng() * 34));
  placeRandom(
    'lone',
    vegetation.loneCount || 0,
    460,
    vegetation.loneMix,
    () => 0,
    () => 1);

  const rimCount = vegetation.rimCount || 0;
  for (let index = 0; index < rimCount; index++) {
    const angle = ((index + (rng() - 0.5)) / Math.max(1, rimCount)) * Math.PI * 2;
    const radius = 442 + rng() * 42;
    const x = Math.cos(angle) * radius;
    const z = Math.sin(angle) * radius;
    addStand(
      'rim',
      x,
      z,
      13 + rng() * 22,
      6 + Math.floor(rng() * 22),
      pickSpecies(vegetation.rimMix, rng));
  }

  for (const belt of vegetation.belts || []) {
    const length = Math.hypot(belt.x1 - belt.x0, belt.z1 - belt.z0);
    const count = Math.max(2, Math.round(length / (belt.gap || 8)));
    for (let index = 0; index <= count; index++) {
      const t = index / count;
      const jitter = belt.jitter ?? 2.5;
      const x = belt.x0 + (belt.x1 - belt.x0) * t + (rng() - 0.5) * jitter;
      const z = belt.z0 + (belt.z1 - belt.z0) * t + (rng() - 0.5) * jitter;
      if (rng() < (belt.skip ?? 0.12) || !clear(x, z, 1)) continue;
      addStand(
        'belt',
        x,
        z,
        0,
        1,
        belt.species || pickSpecies(vegetation.loneMix, rng));
    }
  }

  return {
    stands,
    treeCount: stands.reduce((sum, stand) => sum + stand.count, 0),
  };
}

function mapRecord(id) {
  const config = getMapConfig(id);
  const layout = createLayout(config);
  const minimap = config.minimap || {};
  const fallbackGround = { r: 0.28, g: 0.34, b: 0.22 };
  const structures = structureLayout(config, layout);
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
    unityStructures: structures,
    unityVegetation: vegetationLayout(config, layout, structures),
  };
}

const payload = canonical({
  schemaVersion: 5,
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
  loadout: {
    equipment: EQUIPMENT_CATALOG.map((item) => ({
      id: item.id,
      name: item.name,
      shortName: item.short,
      category: item.cat,
      era: item.era,
      description: item.desc,
    })),
    camouflage: CAMO_CATALOG_PATTERN_IDS.map((id) => ({
      id,
      name: CAMO_PATTERN_LABEL[id],
    })),
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
