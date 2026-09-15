import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import {
  baseCamoOf,
  checkIntervalS,
  combineCamo,
  fireBloomAt,
  spotRangeM,
  viewRangeOf,
} from '../src/sim/spotting.ts';
import {
  specialActionKind,
} from '../src/sim/specialActionPolicy.ts';

const ROOT = resolve(import.meta.dirname, '..');
const OUT = resolve(
  ROOT,
  'Assets/ClaudeOfTanks/Tests/Fixtures/ts-golden-parity.json',
);

const round = (value) => Math.round(value * 1_000_000) / 1_000_000;

function build() {
  const spottingSpecs = [
    { id: 'm1a2', role: 'mbt' },
    { id: 'tiger1', role: 'heavy' },
    { id: 'unknown_light', role: 'light' },
    { id: 'unknown_medium', role: 'medium' },
    { id: 'unknown_td', role: 'td' },
  ];
  const camoCases = [
    {
      id: 'still_open',
      base: 0.23,
      paint: 0.035,
      equip: 0.14,
      firedAtS: 0,
      timeS: 100,
      bush: 0,
      fireLoss: 0.82,
    },
    {
      id: 'fresh_shot_tree',
      base: 0.17,
      paint: 0.035,
      equip: 0.02,
      firedAtS: 0,
      timeS: 0,
      bush: 0.08,
      fireLoss: 0.795,
    },
    {
      id: 'decayed_double_bush',
      base: 0.3,
      paint: 0,
      equip: 0.12,
      firedAtS: 0,
      timeS: 1.7,
      bush: 0.5,
      fireLoss: 0.9,
    },
  ];
  const specialActionCases = [
    { id: 'none', spec: {} },
    { id: 'hydropneumatic', spec: { hydropneumaticAim: {} } },
    {
      id: 'guided_choice',
      spec: { gun: { shells: [{ guided: false }, { guided: true }] } },
    },
    {
      id: 'guided_only',
      spec: { gun: { primaryGuided: true, shells: [{ guided: true }] } },
    },
    { id: 'magazine', spec: { gun: { autoloader: {} } } },
  ];
  const data = {
    version: 1,
    generatedBy: 'tools/generate-unity-parity-vectors.mjs',
    spotting: {
      viewRange: spottingSpecs.map((spec) => ({
        id: spec.id,
        role: spec.role,
        expected: round(viewRangeOf(spec)),
      })),
      baseCamo: spottingSpecs.flatMap((spec) => [
        {
          id: spec.id,
          role: spec.role,
          moving: false,
          expected: round(baseCamoOf(spec, false)),
        },
        {
          id: spec.id,
          role: spec.role,
          moving: true,
          expected: round(baseCamoOf(spec, true)),
        },
      ]),
      checkInterval: [40, 119.99, 120, 279.99, 280, 445].map((distanceM) => ({
        distanceM,
        expected: round(checkIntervalS(distanceM)),
      })),
      spotRange: [
        { viewRangeM: 445, targetCamo: 0 },
        { viewRangeM: 445, targetCamo: 0.35 },
        { viewRangeM: 370, targetCamo: 0.95 },
        { viewRangeM: 40, targetCamo: 0.5 },
      ].map((entry) => ({
        ...entry,
        expected: round(spotRangeM(entry.viewRangeM, entry.targetCamo)),
      })),
      effectiveCamo: camoCases.map((entry) => {
        const bloom = fireBloomAt(entry.firedAtS, entry.timeS);
        return {
          ...entry,
          bloom: round(bloom),
          expected: round(combineCamo({ ...entry, bloom })),
        };
      }),
    },
    specialActions: specialActionCases.map((entry) => ({
      id: entry.id,
      expected: specialActionKind(entry.spec),
    })),
  };
  return JSON.stringify(data, null, 2) + '\n';
}

const next = build();
if (process.argv.includes('--check')) {
  const current = readFileSync(OUT, 'utf8');
  if (current !== next) {
    console.error('[unity-parity] fixture is stale:', OUT);
    process.exit(1);
  }
  console.log('[unity-parity] fixture is current');
} else {
  writeFileSync(OUT, next);
  console.log('[unity-parity] wrote', OUT);
}
