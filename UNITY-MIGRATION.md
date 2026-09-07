# Unity 2022.3 migration

This branch adds a native C# runtime that opens in Unity `2022.3.62f3`. The
existing TypeScript source remains as the behavior and content reference during
the incremental port.

## Run

1. In Unity Hub, choose **Open** and select this repository root.
2. Use Unity `2022.3.62f3`.
3. Open `Assets/Scenes/Battle.unity`, then enter Play Mode.

`GameBootstrap` opens the garage. Select a vehicle, map, and mode, then deploy.
The current desktop controls are WASD or arrow keys to drive, Ctrl to brake,
mouse to aim, left mouse or Space to fire, Shift to toggle sniper mode, and
the mouse wheel to traverse the arcade/sniper zoom ladder.

Command-line verification:

```bash
tools/uloop.sh compile
tools/uloop.sh run-tests --test-mode EditMode
tools/uloop.sh control-play-mode --action Play
tools/uloop.sh screenshot --window-name Game --capture-mode GameView
```

Unity CLI Loop `3.4.0` and Input System `1.7.0` are project dependencies.
The CLI is installed at `~/.local/bin/uloop`; `tools/uloop.sh` binds it to this
project and uses the locally cached project runner when GitHub API rate limits
prevent automatic runner acquisition.

When running inside an IDE sandbox, launch Unity from Unity Hub or macOS
LaunchServices first. Do not use `uloop launch` from the sandbox: Unity's
`bee_backend` requires process and semaphore behavior that the sandbox blocks.
Once the Editor is open, every other `tools/uloop.sh` command is supported.

## Architecture

| Assembly | Responsibility |
| --- | --- |
| `ClaudeOfTanks.Simulation` | Deterministic fixed-step movement, ballistics, penetration, damage, entities, and seeded RNG. It has `noEngineReferences: true`. |
| `ClaudeOfTanks.Runtime` | Unity input, procedural rendering, camera, HUD, scene lifecycle, and simulation-to-view synchronization. |
| `ClaudeOfTanks.Simulation.Tests` | Unity EditMode tests for coordinate conventions, determinism, movement, penetration, and swept shell hits. |

The port preserves the source project's runtime units and conventions:

- meters, seconds, and radians;
- world `+Y` is up;
- tank local `+Z` is forward;
- fixed authoritative simulation step is `1/60` second;
- authoritative randomness is seeded and does not use `UnityEngine.Random`;
- simulation code does not depend on `UnityEngine`, GameObjects, PhysX, or wall-clock time.

## Implemented

- Unity 2022.3 project/package metadata
- Pure C# deterministic simulation assembly
- Acceleration, braking, reverse, pivot steering, terrain traction and turret traverse
- Swept projectiles, gravity, deterministic dispersion, guidance and 2 km penetration
- Plate slope, normalization, overmatch, ricochet and directional armor
- Health, modules, crew, fire, repair, ammunition and autoloader reload channels
- Proximity/FOV/occlusion spotting and deterministic AI input
- Programmatic battlefield, first-party primitive tank rigs, shell visuals
- Chase camera, mouse aim, HUD, battle result, restart
- EditMode simulation tests
- Generated parity catalog for all 165 saved vehicle records, 136 release
  vehicles, 126 production vehicles, and all 20 battlefield configurations
- Runtime `TankSpec` construction and dimension/role/family-driven procedural
  presentation for every production vehicle
- Data-driven map environment, authored spawns, landforms, props, vegetation,
  sky and authoritative terrain height queries for all 20 maps
- Standard, Capture the Flag, Zone Control, Turbo Ball, and Endless Horde modes
- Equipment, consumables, deterministic replay recording, responsive HUD,
  keyboard, gamepad, and touch input
- Bounded battle feedback with 24 pooled spatial audio/particle voices, two
  dynamic lights, and 48 target-attached penetration/ricochet decals
- Garage-first lifecycle with all 126 production vehicles, 20 maps, and five
  modes selectable before deployment, plus complete return-to-garage cleanup
- Arcade chase and sniper camera modes with the source zoom ladder, scoped FOV,
  near-aim protection, vehicle hiding, and HUD scope treatment
- Authoritative module/crew/fire damage status and battle result summary with
  replay-safe Battle Again and return-to-garage actions
- `npm run unity:content:update` / `unity:content:check` drift gate

## Remaining parity work

The original release is substantially larger than this first playable port.
These systems still use the TypeScript implementation as their specification:

- complete 126-vehicle production fleet and authored armor/module geometry;
- all 20 terrain/map recipes, destructibles, vegetation, and streaming;
- progression, loadout editing, replay browser, and production garage;
- input rebinding, settings, minimap, damage panel, and production UI polish;
- audio, particles, decals, postprocessing, and adaptive quality;
- authoritative multiplayer transport, snapshots, prediction, and persistence;
- procedural vehicle geometry parity and generated technical assets.

Migrate these by extending the simulation contracts rather than moving
authority into MonoBehaviours or PhysX. The TypeScript project should remain
buildable until each replacement has parity tests.
