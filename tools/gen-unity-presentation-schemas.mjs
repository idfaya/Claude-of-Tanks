import { mkdir, readFile, writeFile } from 'node:fs/promises';
import * as THREE from 'three';
import '../src/vehicles/tankFactory.ts';
import { createTank } from '../src/vehicles/tankFactory.ts';

const outputUrl = new URL(
  '../Assets/ClaudeOfTanks/Resources/Generated/tank-presentation-schemas.json',
  import.meta.url,
);
const check = process.argv.includes('--check');
const idsArg = process.argv.find((arg) => arg.startsWith('--ids='));
const ids = idsArg ? idsArg.slice('--ids='.length).split(',').filter(Boolean) : ['t90'];

const EXCLUDED_PREFIXES = [
  'procShadow_',
  'vehicleMarking_',
];

const HIDDEN_BASE_NAMES = [
  'Hull',
  'UpperHull',
  'Turret',
  'Gun',
  'Armor-upper_glacis',
  'Armor-lower_front',
  'Armor-hull_side_upper_R',
  'Armor-hull_side_upper_L',
  'Armor-hull_side_lower_R',
  'Armor-hull_side_lower_L',
  'Armor-skirt_rubber_R',
  'Armor-skirt_rubber_L',
  'Armor-track_R',
  'Armor-track_L',
  'Armor-slat_cage',
  'Armor-hull_rear',
  'Armor-hull_roof',
  'Armor-turret_cheek_R',
  'Armor-turret_cheek_L',
  'Armor-mantlet',
  'Armor-turret_side_R',
  'Armor-turret_side_L',
  'Armor-turret_bustle',
  'Armor-turret_roof',
  'Armor-turret_cupola_01_front',
  'Armor-turret_cupola_01_rear',
  'Armor-turret_cupola_01_right',
  'Armor-turret_cupola_01_left',
  'Armor-turret_cupola_01_top',
];

const HIDDEN_BASE_PREFIXES = [
  'Soviet-',
  'Painted-Soviet-',
  'RunningGear-',
  'RoadWheel-',
  'WheelHub-',
  'SuspensionArm-',
  'SuspensionJoint-',
  'ReturnRoller-',
  'Sprocket-',
  'Idler-',
  'TrackLinks-',
];

function rounded(value) {
  if (!Number.isFinite(value)) return 0;
  return Math.round(value * 10000) / 10000;
}

function colorOf(material) {
  const source = Array.isArray(material) ? material[0] : material;
  const color = source?.color;
  return {
    r: rounded(color?.r ?? 0.32),
    g: rounded(color?.g ?? 0.36),
    b: rounded(color?.b ?? 0.26),
  };
}

function targetFor(object, rigs) {
  let current = object;
  while (current) {
    if (current === rigs.gun || current.name === 'rig_recoil') return 'gunFittings';
    if (current === rigs.turret) return 'turret';
    current = current.parent;
  }
  return 'root';
}

function targetRoot(target, rigs) {
  if (target === 'gunFittings') return rigs.gun;
  if (target === 'turret') return rigs.turret;
  return rigs.root;
}

function shouldExport(object) {
  if (!(object.isMesh || object.isInstancedMesh)) return false;
  if (!object.geometry?.attributes?.position) return false;
  return !EXCLUDED_PREFIXES.some((prefix) => object.name.startsWith(prefix));
}

function meshRecord(object, instanceIndex, matrixWorld, target, targetMatrixInverse) {
  const sourceGeometry = object.geometry;
  const position = sourceGeometry.attributes.position;
  const index = sourceGeometry.index;
  const localMatrix = targetMatrixInverse.clone().multiply(matrixWorld);
  const vertex = new THREE.Vector3();
  const vertices = [];
  for (let i = 0; i < position.count; i++) {
    vertex.fromBufferAttribute(position, i).applyMatrix4(localMatrix);
    vertices.push(rounded(vertex.x), rounded(vertex.y), rounded(vertex.z));
  }
  const triangles = [];
  if (index) {
    for (let i = 0; i < index.count; i++) triangles.push(index.getX(i));
  } else {
    for (let i = 0; i < position.count; i++) triangles.push(i);
  }
  const suffix = instanceIndex == null ? '' : `_${instanceIndex}`;
  return {
    name: `TS-T90-${object.name || 'mesh'}${suffix}`,
    target,
    color: colorOf(object.material),
    vertices,
    triangles,
  };
}

function vehicleRecord(id) {
  const visual = createTank(id, {}, {
    materialMode: 'geometry-only',
    geometryQuality: 'high',
    batchStatic: false,
    proceduralOnly: true,
  });
  try {
    visual.root.updateMatrixWorld(true);
    const rigs = {
      root: visual.root,
      turret: visual.root.getObjectByName('rig_turret'),
      gun: visual.root.getObjectByName('rig_gun'),
    };
    if (!rigs.turret || !rigs.gun) {
      throw new Error(`${id}: missing rig_turret or rig_gun`);
    }

    const meshes = [];
    visual.root.traverse((object) => {
      if (!shouldExport(object)) return;
      const target = targetFor(object, rigs);
      const targetInverse = targetRoot(target, rigs).matrixWorld.clone().invert();
      if (object.isInstancedMesh) {
        const instance = new THREE.Matrix4();
        for (let i = 0; i < object.count; i++) {
          object.getMatrixAt(i, instance);
          meshes.push(meshRecord(
            object,
            i,
            object.matrixWorld.clone().multiply(instance),
            target,
            targetInverse,
          ));
        }
      } else {
        meshes.push(meshRecord(
          object,
          null,
          object.matrixWorld,
          target,
          targetInverse,
        ));
      }
    });
    return {
      id,
      source: 'src/vehicles/profiles/t90.ts via createTank geometry-only',
      markerName: 'T90-TsGeneratedPresentationSchema',
      gunFittingsRootName: 'T90-TsGunFittings',
      hiddenNames: HIDDEN_BASE_NAMES,
      hiddenPrefixes: HIDDEN_BASE_PREFIXES,
      meshes,
      meshCount: meshes.length,
      vertexCount: meshes.reduce((sum, mesh) => sum + mesh.vertices.length / 3, 0),
      triangleCount: meshes.reduce((sum, mesh) => sum + mesh.triangles.length / 3, 0),
    };
  } finally {
    visual.dispose?.();
  }
}

function canonical(value) {
  if (Array.isArray(value)) return value.map(canonical);
  if (!value || typeof value !== 'object') return value;
  return Object.fromEntries(Object.keys(value).sort().map((key) => [key, canonical(value[key])]));
}

const payload = canonical({
  schemaVersion: 1,
  vehicles: ids.map(vehicleRecord),
});
const text = `${JSON.stringify(payload)}\n`;

if (check) {
  const current = await readFile(outputUrl, 'utf8');
  if (current !== text) {
    console.error('Unity presentation schema is stale; run npm run unity:presentation:update');
    process.exit(1);
  }
  console.log(`[unity-presentation] PASS ${ids.join(',')}`);
} else {
  await mkdir(new URL('.', outputUrl), { recursive: true });
  await writeFile(outputUrl, text);
  console.log(`[unity-presentation] wrote ${ids.join(',')} -> ${outputUrl.pathname}`);
}
