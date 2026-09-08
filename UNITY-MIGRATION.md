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
tools/uloop.sh run-tests --test-mode PlayMode
tools/uloop.sh control-play-mode --action Play
tools/uloop.sh screenshot --window-name Game --capture-mode GameView
```

Dedicated server build and launch:

```bash
npm run unity:server:build -- linux
./Build/Server/Linux/ClaudeOfTanksServer \
  -batchmode -nographics --cot-server \
  --cot-bind=0.0.0.0 --cot-port=18791 \
  --cot-origins=https://game.example \
  --cot-rating-file=/var/lib/claude-of-tanks/ratings.bin
```

The same listener serves `/match` WebSocket upgrades and the browser-compatible
`/healthz`, `/ranked/identity`, `/ranked/profile/:id`,
`/ranked/leaderboard`, and `/ranked/queue/:id` HTTP API. Environment
equivalents are `COT_SERVER_BIND`, `COT_SERVER_PORT`,
`COT_ALLOWED_ORIGINS`, and `COT_RATING_FILE`.

Install the matching Unity Linux Dedicated Server Build Support module before
running the build command. `tools/build-unity-server.sh macos <output> player`
builds a regular macOS player for local `-batchmode -nographics` smoke tests
when the Dedicated Server module is unavailable.

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
| `ClaudeOfTanks.Server` | Headless composition root for generated match content, ranked HTTP matchmaking, persistent ratings, and dedicated WebSocket sessions. |
| `ClaudeOfTanks.WebRTC` | Native WebRTC peer transport adapter with reliable control and replaceable zero-retransmit state channels. |
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
- Source-configured vegetation manifests for all 20 maps with 4,579
  cluster/lone/rim/belt stands and 65,170 trees. Runtime expansion uses stable
  per-stand seeds, terrain grounding, species-shaped low-poly crowns, sixteen
  distance-managed chunks, and at most 49 merged vegetation meshes per map.
- All 65,170 rendered trees share their exact placement with authoritative
  trunk collision. A fixed spatial grid bounds tank/shell queries; rammed or
  shot trees topple immediately, persist in replay/network destruction state,
  leave no invisible blocker, and rebuild only merged mesh index buffers plus
  one shared fallen-tree mesh.
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
- Deterministic durability and destruction for all 60 tactical structures:
  destroyed obstacles stop blocking movement, shells, and vision; replay seek
  restores their state; merged structure meshes remove only the affected
  geometry and replace it with one shared deterministic debris mesh.
- Standard, Capture the Flag, Zone Control, Turbo Ball, and Endless Horde modes
- Equipment, consumables, deterministic replay recording, responsive HUD,
  keyboard, gamepad, and touch input
- Bounded layered battle feedback with 24 pooled muzzle/penetration/ricochet/
  HE/structure/destruction particle voices, deterministic core/spark/cloud
  channels, two dynamic lights, strict visual-event routing, and 48 pooled
  procedural-atlas armor marks. Penetration holes use molten rims and spall,
  ricochets use tangent-aligned gouges, blunt hits use scuffs, and HE/HESH use
  scorches; marks resolve the outermost visible armor surface, follow the
  nearest hull/turret articulation frame, and clear into a ground scorch when
  the vehicle is destroyed
- Viewer-filtered persistent battle FX with bounded 16-tank dust/exhaust/fire/
  smoke emitters, deterministic particle seeds, 96 fading paired-track prints,
  destruction scorches, reduced-motion budgets, and reset/rematch cleanup
- Shared Garage/solo/network Built-in RP post-processing with bounded reusable
  bloom targets, ACES output grading, saturation/contrast/vignette, crisp
  overlay UI, shader-missing passthrough, and a reversible trim-resolution-tier
  quality governor that preserves the user's persisted quality choice
- Dedicated solo/network battle audio presentation with 24 bounded spatial
  combat voices, ten distance-ranked engine loops that always retain the
  occupied vehicle, a battlefield ambience loop, Killcam ducking, and live
  persistent engine/combat/ambience/UI/voice channel controls
- Canonical reload mechanical sequences for rapid, shell, intra-clip, and
  magazine cycles, with four bounded local voices, ready cues, live combat
  mix/Killcam ducking, authoritative solo state, and network snapshot fallback
- Shared Garage/Battle button feedback on the persistent UI channel plus a
  settings-backed battle pause overlay: solo simulation/input freezes without
  clock debt, network play continues with neutral braking commands, combat and
  engine beds duck to four percent, and UI audio remains foreground
- Complete 36-call/82-variant crew radio payload with one-voice playback,
  two-call priority queue, interruption/cooldown/staleness discipline, live
  voice mix, battle/result/reload/damage/repair/awareness routing, and
  server-authoritative network module/crew/spotted-state replication
- Garage-first lifecycle with all 126 production vehicles, 20 maps, and five
  modes selectable before deployment, plus complete return-to-garage cleanup
- Production Garage presentation with an enclosed steel workshop, hazard-rim
  turntable, roof trusses, three warm high-bay fixtures, canonical hero
  staging, restrained tactical selection/command rails, and a live
  vehicle/map/loadout inspector
- Data-driven Garage loadout editor generated from the canonical TypeScript
  catalog, with 14 era/vehicle-gated equipment choices, three slots, all 112
  match-safe camouflage choices, per-vehicle persistence, live preview paint,
  and selection handoff to solo, private-room, and ranked battles
- Deterministic Unity camouflage textures generated from all 112 canonical
  TypeScript recipes, including national Factory and per-vehicle Signature
  resolution, vehicle-scale tiling, projected armor UVs, and shared rendering
  across Garage, solo, network battle, Killcam, and archived replay surfaces
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
- Persistent accessibility controls for reduced motion, high contrast, and
  0.8x-1.25x HUD text scaling; live keyboard/gamepad action prompts; standard
  controller LT scope, RT fire, A brake, and X/Y/B consumable mappings; and
  reduced-motion combat particles with dynamic flash suppression
- Authoritative post-battle replay playback with seekable deterministic
  simulation, last-eight-second Killcam, full-current-battle replay, timeline
  status, and restoration of the final live battle state
- Persistent replay archive with strict 64 MiB binary simulation codec,
  SHA-256-verified compressed atomic files, 12-match retention, corrupt-file
  isolation, Garage browse/play/delete controls, and backward-compatible
  per-vehicle equipment/camouflage restoration
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
- Snapshot payload v2 persistent structure/tree destruction with stable bounded
  obstacle indices, monotonic revisions, delta-only additions, v1 decode
  compatibility, and full keyframe recovery for reconnecting clients
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
- Unity headless bootstrap with strict bind/port/origin configuration, a
  bounded 60 Hz service pump, UI-free Server builds, and reproducible
  Linux/macOS command-line build tooling
- Same-origin ranked HTTP and dedicated WebSocket transport with bounded
  headers, JSON bodies, concurrent admissions and per-client request rates;
  CORS policy applies before either transport is admitted
- Official Unity WebRTC `3.0.0-pre.8` native transport for Editor, macOS,
  Windows, Linux, Android, and iOS. Private matches use reliable ordered
  `cot-match-v1` control and unordered zero-retransmit `cot-state-v1` state
  channels behind the shared `INetworkTransportEndpoint`; control queues are
  bounded and congested state packets coalesce to the latest packet.
- Signaling-neutral Unity peer sessions create and validate bounded
  offer/answer/ICE messages, queue candidates until a remote description is
  installed, replay answers for duplicate durable offers, reject overlapping
  negotiation, validate relay-only TURN configuration, and expose the shared
  transport only after both channels are open.
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
- per-family structure geometry/material parity, complete vegetation recipes,
  and broader world streaming for all 20 maps;
- remaining production UI polish;
- remaining AO/aerial perspective parity;
- installable build-target release artifacts, WebRTC signaling/private-room
  session composition, and signaling deployment;
- per-family procedural vehicle geometry parity and generated technical assets.

Migrate these by extending the simulation contracts rather than moving
authority into MonoBehaviours or PhysX. The TypeScript project should remain
buildable until each replacement has parity tests.
