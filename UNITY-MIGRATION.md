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
| `ClaudeOfTanks.Network` | Transport-independent input admission, authoritative match ticking, and viewer-filtered snapshots. It has `noEngineReferences: true`. |
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
- Shared authoritative/rendered height fields for all 20 maps with sixteen
  bounded terrain chunks per battlefield, analytic normals, exact seam
  vertices, terrain-draped roads/marshes/craters/scatter, and authored
  horizontal water levels.
- Source-layout-driven surface presentation for all 20 maps: country/grid/path
  road networks with casings, lake and frozen-water sheets, marsh/soft-ground
  discs, map palettes, ground variation, and all 1,420 configured craters.
  Each surface class is merged into a bounded presentation-only mesh.
- Source-plan-driven structure presentation for all 20 maps: 601 road-aligned
  planned buildings, 60 exact tactical landmarks, 197 wall runs, 894 rubble
  piles, 361 sandbag lines, and 346 hedgehogs in five merged material buckets.
- Pure C# authoritative structure manifests for all 20 maps with 1,545 bounded
  building, ruin-section, and wall-piece OBBs. The shared deterministic queries
  block tanks and shells, occlude bot/HUD/network spotting, preserve authored
  wall gaps, and round-trip through backward-compatible replay codec v2.
- Standard, Capture the Flag, Zone Control, Turbo Ball, and Endless Horde modes
- Equipment, consumables, deterministic replay recording, responsive HUD,
  keyboard, gamepad, and touch input
- Bounded battle feedback with 24 pooled spatial audio/particle voices, two
  dynamic lights, and 48 target-attached penetration/ricochet decals
- Garage-first lifecycle with all 126 production vehicles, 20 maps, and five
  modes selectable before deployment, plus complete return-to-garage cleanup
- Shared production-fleet running gear with closed discrete-link track meshes,
  road-wheel hubs, sprockets, idlers, return rollers, and inboard suspension
  arms/joints derived from each vehicle's dimensions and track width
- Arcade chase and sniper camera modes with the source zoom ladder, scoped FOV,
  near-aim protection, vehicle hiding, and HUD scope treatment
- Authoritative module/crew/fire damage status and battle result summary with
  replay-safe Battle Again and return-to-garage actions
- Responsive 20-map tactical minimap with source road/water/soft-ground
  cartography, ten-by-ten grid, player/allied headings, spotting-gated enemy
  markers, and CTF/zone/Turbo Ball objective markers
- Responsive touch battle controls with independent held-direction state,
  drag-to-aim, fire, brake, sniper toggle, orientation-aware consumables, and
  separate landscape/portrait safe-area layouts
- Shared Garage/battle settings with persistent master volume, quality and
  fullscreen controls plus conflict-safe keyboard rebinding for movement,
  brake, fire, sniper mode, and all three consumables
- Authoritative post-battle replay playback with seekable deterministic
  simulation, last-eight-second Killcam, full-current-battle replay, timeline
  status, and restoration of the final live battle state
- Persistent replay archive with strict 64 MiB binary simulation codec,
  SHA-256-verified compressed atomic files, 12-match retention, corrupt-file
  isolation, and Garage browse/play/delete controls
- Renderer-free multiplayer authority baseline with bounded 60 Hz catch-up,
  validated sequenced input, deduplicated action edges, 20 Hz snapshot cadence,
  spectator views, and pre-serialization spotting filters
- Bounded client snapshot buffering with shortest-angle interpolation, 250 ms
  extrapolation ceiling, immediate visibility removal, and shared-movement
  local prediction with acknowledged-input replay and presentation correction
- Versioned binary full-snapshot codec with 64 KiB packet, entity, shell, event,
  string, enum, finite-number, truncation, and trailing-data validation
- ACK-based keyframe/delta snapshots with explicit visibility removals,
  missing-base recovery, bounded history, and strict binary frame validation
- Bounded dual-channel loopback transport and host/client pumps: reliable FIFO
  input, replaceable latest-state delivery, prediction reconciliation, and
  automatic keyframe recovery after state loss
- Transport-independent C# host/client pumps plus a real `ClientWebSocket`
  adapter with versioned binary lane framing, bounded control queues,
  replaceable state backpressure, fragmented-frame assembly, strict rejection,
  and main-thread event dispatch
- Native C# dedicated WebSocket session service with bounded RFC 6455 upgrade,
  exact origin policy, one-time ticket and rotating reconnect authentication,
  stale-generation isolation, per-viewer snapshots, and one authoritative tick
  per match regardless of connected player count
- Persistent authoritative room policy for 1v1 through 7v7, spectators,
  readiness and selection locks, host-owned rules, round retention, reserved
  disconnect seats, hashed rotating resume tokens, and deterministic host
  migration
- Server-owned anonymous ranked identities, SHA-256 bearer authentication,
  bounded Elo and rank tiers, idempotent match settlement, secret-free
  leaderboards, and strict atomically replaced binary persistence
- Expanding-band ranked queues for 1v1/2v2/3v3/5v5/7v7, deterministic
  rating-balanced teams, complete map rotation, authenticated queue polling,
  one-time match tickets, rotating reconnect sessions, stale-generation
  isolation, and bounded match/result reclamation
- `npm run unity:content:update` / `unity:content:check` drift gate

## Remaining parity work

The original release is substantially larger than this first playable port.
These systems still use the TypeScript implementation as their specification:

- complete 126-vehicle production fleet and authored armor/module geometry;
- per-family structure geometry/material parity, authoritative structure
  destruction/debris synchronization, complete vegetation recipes, and world
  streaming for all 20 maps;
- progression, loadout editing, and production garage;
- controller glyph polish, accessibility options, and production UI polish;
- audio, particles, decals, postprocessing, and adaptive quality;
- Unity headless build/bootstrap, ranked HTTP matchmaking endpoints,
  WebRTC private-room transport, and signaling deployment;
- per-family procedural vehicle geometry/pattern parity and generated technical assets.

Migrate these by extending the simulation contracts rather than moving
authority into MonoBehaviours or PhysX. The TypeScript project should remain
buildable until each replacement has parity tests.
