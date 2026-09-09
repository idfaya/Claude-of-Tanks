import * as THREE from 'three';
import '../src/vehicles/tankFactory.ts';
import { createTank } from '../src/vehicles/tankFactory.ts';

const EXCLUDED_PREFIXES = [
  'procShadow_',
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

type TextureRecord = {
  readonly path: string;
  readonly dataUrl: string;
};

type ExportResult = {
  readonly payload: unknown;
  readonly textures: readonly TextureRecord[];
};

function rounded(value: number): number {
  if (!Number.isFinite(value)) return 0;
  return Math.round(value * 10000) / 10000;
}

function colorOf(material: THREE.Material | THREE.Material[] | null | undefined) {
  const source = Array.isArray(material) ? material[0] : material;
  const color = (source as THREE.MeshStandardMaterial | undefined)?.color;
  return {
    r: rounded(color?.r ?? 0.32),
    g: rounded(color?.g ?? 0.36),
    b: rounded(color?.b ?? 0.26),
  };
}

function colorRecord(
  color: THREE.Color | undefined,
  fallback: { readonly r: number; readonly g: number; readonly b: number },
) {
  if (!color) return fallback;
  return {
    r: rounded(color.r ?? fallback.r),
    g: rounded(color.g ?? fallback.g),
    b: rounded(color.b ?? fallback.b),
  };
}

function targetFor(object: THREE.Object3D, rigs: Record<string, THREE.Object3D>) {
  let current: THREE.Object3D | null = object;
  while (current) {
    if (current === rigs.gun || current.name === 'rig_recoil') return 'gunFittings';
    if (current === rigs.turret) return 'turret';
    current = current.parent;
  }
  return 'root';
}

function targetRoot(target: string, rigs: Record<string, THREE.Object3D>) {
  if (target === 'gunFittings') return rigs.gun;
  if (target === 'turret') return rigs.turret;
  return rigs.root;
}

function shouldExport(object: THREE.Object3D): object is THREE.Mesh | THREE.InstancedMesh {
  if (!('isMesh' in object || 'isInstancedMesh' in object)) return false;
  const mesh = object as THREE.Mesh | THREE.InstancedMesh;
  if (!mesh.geometry?.attributes?.position) return false;
  return !EXCLUDED_PREFIXES.some((prefix) => object.name.startsWith(prefix));
}

function texturePath(
  texture: THREE.Texture | null | undefined,
  usage: string,
  textures: TextureRecord[],
  texturePaths: Map<string, string>,
  id: string,
) {
  const image = texture?.image as HTMLCanvasElement | undefined;
  if (!texture || !image || typeof image.toDataURL !== 'function') return '';
  const existing = texturePaths.get(texture.uuid);
  if (existing) return existing;
  const path =
    `Assets/ClaudeOfTanks/Generated/PresentationSource/Textures/` +
    `${id}-${String(textures.length).padStart(2, '0')}-${usage}.png`;
  textures.push({
    path,
    dataUrl: image.toDataURL('image/png'),
  });
  texturePaths.set(texture.uuid, path);
  return path;
}

function materialRecord(
  material: THREE.Material | THREE.Material[] | null | undefined,
  object: THREE.Object3D,
  textures: TextureRecord[],
  texturePaths: Map<string, string>,
  id: string,
) {
  const source = Array.isArray(material) ? material[0] : material;
  const standard = source as THREE.MeshStandardMaterial | undefined;
  const physical = source as THREE.MeshPhysicalMaterial | undefined;
  const userData = source?.userData ?? {};
  return {
    name: source?.name ?? '',
    type: source?.type ?? '',
    appearanceRole: userData.appearanceRole ?? object.userData?.appearanceRole ?? '',
    vehicleMaterialRole: userData.vehicleMaterialRole ?? '',
    camoProjection: userData.camoProjection ?? '',
    camoUvScale: rounded(userData.camoUvScale ?? 0),
    color: colorOf(source),
    emissive: colorRecord(standard?.emissive, { r: 0, g: 0, b: 0 }),
    emissiveIntensity: rounded(standard?.emissiveIntensity ?? 1),
    roughness: rounded(standard?.roughness ?? 0.5),
    metalness: rounded(standard?.metalness ?? 0),
    clearcoat: rounded(physical?.clearcoat ?? 0),
    clearcoatRoughness: rounded(physical?.clearcoatRoughness ?? 0),
    specularIntensity: rounded(physical?.specularIntensity ?? 1),
    opacity: rounded(source?.opacity ?? 1),
    transparent: Boolean(source?.transparent),
    side: source?.side ?? THREE.FrontSide,
    vertexColors: Boolean((source as THREE.MeshStandardMaterial | undefined)?.vertexColors),
    hasMap: Boolean(standard?.map),
    mapPath: texturePath(standard?.map, 'albedo', textures, texturePaths, id),
    normalMapPath: texturePath(standard?.normalMap, 'normal', textures, texturePaths, id),
    roughnessMapPath: texturePath(standard?.roughnessMap, 'roughness', textures, texturePaths, id),
    bumpMapPath: texturePath(standard?.bumpMap, 'bump', textures, texturePaths, id),
    emissiveMapPath: texturePath(standard?.emissiveMap, 'emissive', textures, texturePaths, id),
  };
}

function meshRecord(
  object: THREE.Mesh | THREE.InstancedMesh,
  instanceIndex: number | null,
  matrixWorld: THREE.Matrix4,
  target: string,
  targetMatrixInverse: THREE.Matrix4,
  textures: TextureRecord[],
  texturePaths: Map<string, string>,
  id: string,
) {
  const sourceGeometry = object.geometry;
  const position = sourceGeometry.attributes.position;
  const normal = sourceGeometry.attributes.normal;
  const uv = sourceGeometry.attributes.uv;
  const color = sourceGeometry.attributes.color;
  const index = sourceGeometry.index;
  const localMatrix = targetMatrixInverse.clone().multiply(matrixWorld);
  const normalMatrix = new THREE.Matrix3().getNormalMatrix(localMatrix);
  const vertex = new THREE.Vector3();
  const direction = new THREE.Vector3();
  const vertices: number[] = [];
  const normals: number[] = [];
  const uvs: number[] = [];
  const colors: number[] = [];
  for (let i = 0; i < position.count; i++) {
    vertex.fromBufferAttribute(position, i).applyMatrix4(localMatrix);
    vertices.push(rounded(vertex.x), rounded(vertex.y), rounded(vertex.z));
    if (normal) {
      direction.fromBufferAttribute(normal, i).applyNormalMatrix(normalMatrix).normalize();
      normals.push(rounded(direction.x), rounded(direction.y), rounded(direction.z));
    }
    if (uv) {
      uvs.push(rounded(uv.getX(i)), rounded(uv.getY(i)));
    }
    if (color) {
      colors.push(
        rounded(color.getX(i)),
        rounded(color.getY(i)),
        rounded(color.getZ(i)),
      );
    }
  }
  const triangles: number[] = [];
  if (index) {
    for (let i = 0; i < index.count; i++) triangles.push(index.getX(i));
  } else {
    for (let i = 0; i < position.count; i++) triangles.push(i);
  }
  const suffix = instanceIndex == null ? '' : `_${instanceIndex}`;
  return {
    name: `TS-T90-${object.name || 'mesh'}${suffix}`,
    target,
    material: materialRecord(object.material, object, textures, texturePaths, id),
    vertices,
    normals,
    uvs,
    colors,
    triangles,
  };
}

function vehicleRecord(id: string, textures: TextureRecord[]) {
  const visual = createTank(id, {}, {
    materialMode: 'rendered',
    geometryQuality: 'high',
    batchStatic: false,
    proceduralOnly: true,
  });
  try {
    visual.root.updateMatrixWorld(true);
    const rigs = {
      root: visual.root,
      turret: visual.root.getObjectByName('rig_turret')!,
      gun: visual.root.getObjectByName('rig_gun')!,
    };
    if (!rigs.turret || !rigs.gun) {
      throw new Error(`${id}: missing rig_turret or rig_gun`);
    }
    const texturePaths = new Map<string, string>();
    const meshes: ReturnType<typeof meshRecord>[] = [];
    visual.root.traverse((object) => {
      if (!shouldExport(object)) return;
      const target = targetFor(object, rigs);
      const targetInverse = targetRoot(target, rigs).matrixWorld.clone().invert();
      if (object instanceof THREE.InstancedMesh) {
        const instance = new THREE.Matrix4();
        for (let i = 0; i < object.count; i++) {
          object.getMatrixAt(i, instance);
          meshes.push(meshRecord(
            object,
            i,
            object.matrixWorld.clone().multiply(instance),
            target,
            targetInverse,
            textures,
            texturePaths,
            id,
          ));
        }
      } else {
        meshes.push(meshRecord(
          object,
          null,
          object.matrixWorld,
          target,
          targetInverse,
          textures,
          texturePaths,
          id,
        ));
      }
    });
    return {
      id,
      source: 'src/vehicles/profiles/t90.ts via createTank rendered browser bake',
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

export function exportTankPresentation(ids: string[]): ExportResult {
  const textures: TextureRecord[] = [];
  return {
    payload: {
      schemaVersion: 3,
      vehicles: ids.map((id) => vehicleRecord(id, textures)),
    },
    textures,
  };
}
