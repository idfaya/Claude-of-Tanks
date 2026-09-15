import { gzipSync } from 'node:zlib';
import {
  mkdirSync,
  readFileSync,
  writeFileSync,
} from 'node:fs';
import { resolve } from 'node:path';
import * as THREE from 'three';
import { createTank } from '../src/vehicles/tankFactory.ts';

const ROOT = resolve(import.meta.dirname, '..');
const OUTPUT = resolve(
  ROOT,
  'Assets/ClaudeOfTanks/Resources/Content/VehicleGeometry',
);
const args = process.argv.slice(2);
const check = args.includes('--check');
const all = args.includes('--all');

function option(name, fallback = '') {
  const inline = args.find((arg) => arg.startsWith(`--${name}=`));
  if (inline) return inline.slice(name.length + 3);
  const index = args.indexOf(`--${name}`);
  return index >= 0 ? args[index + 1] : fallback;
}

const requested = option('ids');
if (!requested && !all) {
  throw new Error(
    'Specify --ids=<vehicle ids> or --all.',
  );
}
const ids = all
  ? JSON.parse(
      readFileSync(
        resolve(
          ROOT,
          'Assets/ClaudeOfTanks/Resources/Content/content-catalog.json',
        ),
        'utf8',
      ),
    ).vehicles.map((vehicle) => vehicle.id)
  : requested
      .split(',')
      .map((id) => id.trim())
      .filter(Boolean);

function installCanvasStub() {
  if (globalThis.document) return;
  globalThis.Path2D = class {
    addPath() {}
    moveTo() {}
    lineTo() {}
    quadraticCurveTo() {}
    bezierCurveTo() {}
    closePath() {}
    rect() {}
    arc() {}
    ellipse() {}
  };
  const gradient = { addColorStop() {} };
  const context = new Proxy(
    {
      createLinearGradient: () => gradient,
      createRadialGradient: () => gradient,
      createImageData: (width, height) => ({
        width,
        height,
        data: new Uint8ClampedArray(width * height * 4),
      }),
      getImageData: (x, y, width, height) => ({
        width,
        height,
        data: new Uint8ClampedArray(width * height * 4),
      }),
      putImageData() {},
      measureText: (value) => ({
        width: String(value).length * 8,
      }),
    },
    {
      get(target, property) {
        if (property in target) return target[property];
        return () => {};
      },
      set(target, property, value) {
        target[property] = value;
        return true;
      },
    },
  );
  globalThis.document = {
    fonts: null,
    createElement(name) {
      if (name !== 'canvas') {
        throw new Error(`Unsupported geometry-export element: ${name}`);
      }
      return {
        width: 1,
        height: 1,
        getContext: () => context,
      };
    },
  };
}

installCanvasStub();

const OWNER = Object.freeze({
  rig_hull: 0,
  rig_turret: 1,
  rig_gun: 2,
  rig_recoil: 3,
  rig_barrel_0: 4,
  rig_barrel_1: 5,
});

const ROLE = Object.freeze({
  armorPaint: 0,
  wheelPaint: 1,
  tireRubber: 2,
  trackSteel: 3,
  gunmetal: 4,
  opticGlass: 5,
  canvas: 6,
  wood: 7,
  fittingPaint: 8,
  decal: 9,
});
const EXPLICIT_ROLE = new Map([
  ['wheelPaint', ROLE.wheelPaint],
  ['wheelDish', ROLE.wheelPaint],
  ['tireRubber', ROLE.tireRubber],
  ['wheelTire', ROLE.tireRubber],
  ['wheelInset', ROLE.tireRubber],
  ['trackSteel', ROLE.trackSteel],
  ['trackBand', ROLE.trackSteel],
  ['trackPad', ROLE.trackSteel],
  ['trackPadSimplified', ROLE.trackSteel],
  ['trackHardware', ROLE.trackSteel],
  ['opticGlass', ROLE.opticGlass],
  ['canvas', ROLE.canvas],
  ['wood', ROLE.wood],
  ['armorPaint', ROLE.armorPaint],
  ['fittingPaint', ROLE.fittingPaint],
  ['gunmetal', ROLE.gunmetal],
  ['gearShadow', ROLE.gunmetal],
  ['suspensionLink', ROLE.gunmetal],
  ['suspensionJoint', ROLE.gunmetal],
]);
const INFERRED_ROLE = [
  [/^vehicleMarking_/, ROLE.decal],
  [/glass|lens|optic/i, ROLE.opticGlass],
  [/rubber|tire|inset/i, ROLE.tireRubber],
  [/track|tread|shoe|spare/i, ROLE.trackSteel],
  [/dark|shadow|suspension/i, ROLE.gunmetal],
  [/wood/i, ROLE.wood],
  [/cloth|canvas|ghillie|net/i, ROLE.canvas],
  [/detail|equipment|fitting/i, ROLE.fittingPaint],
];

class Writer {
  constructor() {
    this.parts = [];
  }

  bytes(value) {
    this.parts.push(Buffer.from(value));
  }

  u8(value) {
    const buffer = Buffer.allocUnsafe(1);
    buffer.writeUInt8(value);
    this.parts.push(buffer);
  }

  u16(value) {
    const buffer = Buffer.allocUnsafe(2);
    buffer.writeUInt16LE(value);
    this.parts.push(buffer);
  }

  u32(value) {
    const buffer = Buffer.allocUnsafe(4);
    buffer.writeUInt32LE(value);
    this.parts.push(buffer);
  }

  f32(value) {
    const buffer = Buffer.allocUnsafe(4);
    buffer.writeFloatLE(value);
    this.parts.push(buffer);
  }

  string(value) {
    const encoded = Buffer.from(value, 'utf8');
    if (encoded.length > 0xffff) {
      throw new Error(`Recipe string is too long: ${value.slice(0, 80)}`);
    }
    this.u16(encoded.length);
    this.bytes(encoded);
  }

  finish() {
    return Buffer.concat(this.parts);
  }
}

function primaryLodVisible(object, root) {
  let child = object;
  for (let parent = object.parent; parent && parent !== root;
    child = parent, parent = parent.parent) {
    if (!parent.isLOD) continue;
    const primary = parent.levels?.[0]?.object;
    let cursor = child;
    let owned = false;
    while (cursor && cursor !== parent) {
      if (cursor === primary) {
        owned = true;
        break;
      }
      cursor = cursor.parent;
    }
    if (!owned) return false;
  }
  return true;
}

function rigOwner(object, root) {
  for (let cursor = object.parent; cursor && cursor !== root;
    cursor = cursor.parent) {
    if (OWNER[cursor.name] !== undefined) {
      return {
        code: OWNER[cursor.name],
        object: cursor,
      };
    }
  }
  return null;
}

function materialRole(object, material) {
  const explicit = object.userData?.appearanceRole
    || material?.userData?.appearanceRole;
  const resolved = EXPLICIT_ROLE.get(explicit);
  if (resolved !== undefined) return resolved;
  const name = object.name || '';
  return INFERRED_ROLE.find(([pattern]) => pattern.test(name))?.[1]
    ?? ROLE.armorPaint;
}

function matrixList(object, owner) {
  const ownerInverse = owner.matrixWorld.clone().invert();
  const relative = ownerInverse.multiply(object.matrixWorld);
  if (!object.isInstancedMesh) return [relative.elements.slice()];

  const result = [];
  const instance = new THREE.Matrix4();
  for (let index = 0; index < object.count; index++) {
    object.getMatrixAt(index, instance);
    result.push(relative.clone().multiply(instance).elements.slice());
  }
  return result;
}

function sourceRecord(object, root) {
  if (!object.isMesh || !object.visible) return null;
  if (!primaryLodVisible(object, root)) return null;
  if (object.name.startsWith('procShadow_')
      || object.material?.name === 'ProceduralShadowProxy'
      || object.material?.colorWrite === false) {
    return null;
  }
  const owner = rigOwner(object, root);
  if (!owner) return null;
  const geometry = object.geometry;
  const position = geometry?.getAttribute('position');
  if (!position || position.itemSize !== 3 || position.count < 3) return null;
  const normal = geometry.getAttribute('normal');
  const uv = geometry.getAttribute('uv');
  const index = geometry.index;
  const material = Array.isArray(object.material)
    ? object.material[0]
    : object.material;
  const color = material?.color || new THREE.Color(0.4, 0.42, 0.36);
  return {
    name: object.name || 'Mesh',
    owner: owner.code,
    role: materialRole(object, material),
    color: [color.r, color.g, color.b, material?.opacity ?? 1],
    position,
    normal: normal?.itemSize === 3 && normal.count === position.count
      ? normal
      : null,
    uv: uv?.itemSize === 2 && uv.count === position.count
      ? uv
      : null,
    index,
    matrices: matrixList(object, owner.object),
  };
}

function writeAttribute(writer, attribute, itemSize, count) {
  for (let index = 0; index < count; index++) {
    for (let component = 0; component < itemSize; component++) {
      writer.f32(attribute.getComponent(index, component));
    }
  }
}

function writeSource(writer, source) {
  let flags = 0;
  if (source.normal) flags |= 1;
  if (source.uv) flags |= 2;
  if (source.index) flags |= 4;
  writer.u8(source.owner);
  writer.u8(source.role);
  writer.u8(flags);
  writer.u8(0);
  writer.string(source.name);
  writer.u32(source.position.count);
  writer.u32(source.index?.count || 0);
  writer.u16(source.matrices.length);
  writer.u16(0);
  for (const component of source.color) writer.f32(component);
  writeAttribute(writer, source.position, 3, source.position.count);
  if (source.normal) {
    writeAttribute(writer, source.normal, 3, source.position.count);
  }
  if (source.uv) {
    writeAttribute(writer, source.uv, 2, source.position.count);
  }
  if (source.index) {
    for (let index = 0; index < source.index.count; index++) {
      writer.u32(source.index.getX(index));
    }
  }
  for (const matrix of source.matrices) {
    for (const component of matrix) writer.f32(component);
  }
}

function collectGeometry(visual, id) {
  visual.root.updateMatrixWorld(true);
  const hull = visual.root.getObjectByName('rig_hull');
  const turret = visual.root.getObjectByName('rig_turret');
  const gun = visual.root.getObjectByName('rig_gun');
  if (!hull || !turret || !gun) {
    throw new Error(`${id}: authored articulation rig is incomplete`);
  }
  const sources = [];
  const bounds = new THREE.Box3();
  visual.root.traverse((object) => {
    const source = sourceRecord(object, visual.root);
    if (!source) return;
    sources.push(source);
    bounds.union(new THREE.Box3().setFromObject(object));
  });
  return { hull, turret, gun, sources, bounds };
}

function serialize(id) {
  const visual = createTank(id, null, {
    proceduralOnly: true,
    geometryReceipt: true,
    materialMode: 'geometry-only',
    geometryQuality: 'high',
    decor: true,
  });
  try {
    const { hull, turret, gun, sources, bounds } =
      collectGeometry(visual, id);
    const writer = new Writer();
    writer.bytes(Buffer.from('CTG3', 'ascii'));
    writer.u16(3);
    writer.u16(sources.length);
    writer.string(id);
    for (const value of turret.position.toArray()) writer.f32(value);
    for (const value of gun.position.toArray()) writer.f32(value);
    for (const value of hull.scale.toArray()) writer.f32(value);
    for (const value of turret.scale.toArray()) writer.f32(value);
    for (const value of gun.scale.toArray()) writer.f32(value);
    for (const source of sources) writeSource(writer, source);
    return {
      sourceCount: sources.length,
      vertexCount: sources.reduce(
        (sum, source) =>
          sum +
          source.position.count *
          source.matrices.length,
        0,
      ),
      bounds: {
        min: bounds.min.toArray(),
        max: bounds.max.toArray(),
      },
      raw: writer.finish(),
    };
  } finally {
    visual.dispose();
  }
}

mkdirSync(OUTPUT, { recursive: true });
const manifest = {
  schemaVersion: 1,
  generatedBy: 'tools/generate-unity-vehicle-geometry.mjs',
  vehicles: [],
};
for (const id of ids) {
  const {
    sourceCount,
    vertexCount,
    bounds,
    raw,
  } = serialize(id);
  const compressed = gzipSync(raw, { level: 9 });
  const path = resolve(OUTPUT, `${id}.bytes`);
  if (check) {
    const current = readFileSync(path);
    if (!current.equals(compressed)) {
      throw new Error(`${id}: Unity vehicle geometry recipe is stale`);
    }
  } else {
    writeFileSync(path, compressed);
  }
  manifest.vehicles.push({
    id,
    sourceCount,
    vertexCount,
    bounds,
    compressedBytes: compressed.length,
  });
  console.log(
    `[unity-geometry] ${id}: ${sourceCount} sources, ` +
    `${raw.length} raw bytes, ${compressed.length} compressed bytes`,
  );
}
if (all) {
  const manifestPath = resolve(OUTPUT, 'manifest.json');
  const serialized = `${JSON.stringify(manifest, null, 2)}\n`;
  if (check) {
    if (readFileSync(manifestPath, 'utf8') !== serialized) {
      throw new Error('Unity vehicle geometry manifest is stale');
    }
  } else {
    writeFileSync(manifestPath, serialized);
  }
}
