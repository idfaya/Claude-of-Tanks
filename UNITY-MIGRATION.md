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
npm run server:dotnet:build -- linux-x64
./Build/Server/DotNet/linux-x64/ClaudeOfTanks.Server \
  --cot-bind=0.0.0.0 --cot-port=18791 \
  --cot-signal-port=18792 \
  --cot-origins=https://game.example \
  --cot-rating-file=/var/lib/claude-of-tanks/ratings.bin
```

The same listener serves `/match` WebSocket upgrades and the browser-compatible
`/healthz`, `/ranked/identity`, `/ranked/profile/:id`,
`/ranked/leaderboard`, and `/ranked/queue/:id` HTTP API. Environment
equivalents are `COT_SERVER_BIND`, `COT_SERVER_PORT`,
`COT_SIGNAL_PORT`, `COT_ALLOWED_ORIGINS`, `COT_RATING_FILE`, and
`COT_CONTENT_CATALOG`. The same process also starts the native C# private-room
signaling service on `COT_SIGNAL_PORT`; its WebSocket endpoint is `/signal`.

The server is a self-contained .NET 8 executable and does not require Unity,
Unity Hub, the Dedicated Server Build Support module, or a preinstalled .NET
runtime on the deployment host. A .NET 8 SDK is required only on the build
machine. Run `npm run server:dotnet:test` for the local HTTP, ranked queue,
dedicated WebSocket, and private-room signaling smoke test.

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
| `ClaudeOfTanks.Server` | Shared C# service core for generated match content, ranked HTTP matchmaking, persistent ratings, and dedicated WebSocket sessions. |
| `server/dotnet/ClaudeOfTanks.Server` | Unity-independent .NET 8 process entrypoint and self-contained deployment target. |
| `ClaudeOfTanks.WebRTC` | Native WebRTC peer transport adapter with reliable control and replaceable zero-retransmit state channels. |
| `ClaudeOfTanks.Simulation.Tests` | Unity EditMode tests for coordinate conventions, determinism, movement, penetration, and swept shell hits. |

The port preserves the source project's runtime units and conventions:

- meters, seconds, and radians;
- world `+Y` is up;
- tank local `+Z` is forward;
- fixed authoritative simulation step is `1/60` second;
- authoritative randomness is seeded and does not use `UnityEngine.Random`;
- simulation code does not depend on `UnityEngine`, GameObjects, PhysX, or wall-clock time.

## C# ownership boundary

Unity runtime, standalone .NET dedicated server, private-room signaling, content data,
EditMode tests, and PlayMode tests are C#-native and do not launch Node or
execute TypeScript. `CSharpRuntimeBoundaryTests` enforces that boundary.
`Resources/Content/content-catalog.json` is the canonical language-neutral
Unity content asset; C# owns its schema, loading, and validation. TypeScript
remains only as the reference implementation while vehicle presentation
builders are translated into shared C# shape factories and per-vehicle
composition code. Generated TS meshes are not a runtime or repository asset.

## Implemented

- Unity 2022.3 project/package metadata
- Native C# private-room signaling server with origin validation, room
  create/join/leave, session rotation, signal relay, and durable mailbox
  polling. Unity PlayMode coverage no longer launches Node or
  `server/signalingServer.ts`; an EditMode boundary test prevents that runtime
  dependency from returning.
- Pure C# deterministic simulation assembly
- Acceleration, braking, reverse, pivot steering, terrain traction and turret traverse
- Swept projectiles, gravity, deterministic dispersion, guidance and 2 km penetration
- Plate slope, normalization, overmatch, ricochet and directional armor
- Health, modules, crew, fire, repair, ammunition and autoloader reload channels
- Proximity/FOV/occlusion spotting and deterministic AI input
- Programmatic battlefield, first-party primitive tank rigs, shell visuals
- Chase camera, mouse aim, HUD, battle result, restart
- EditMode simulation tests
- Canonical Unity catalog for all 165 saved vehicle records, 136 release
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
  per-stand seeds, terrain grounding, TS-derived archetype proportions for all
  13 catalog species, species-shaped low-poly crowns, sixteen distance-managed
  chunks, and at most 97 merged vegetation meshes per map after palm and
  birch/aspen material buckets.
- All 65,170 rendered trees share their exact placement with authoritative
  trunk collision. A fixed spatial grid bounds tank/shell queries; rammed or
  shot trees topple immediately, persist in replay/network destruction state,
  leave no invisible blocker, and rebuild only merged mesh index buffers plus
  one shared fallen-tree mesh.
- Source-layout-driven surface presentation for all 20 maps: country/grid/path
  road networks with casings, lake and frozen-water sheets, marsh/soft-ground
  discs, map palettes, ground variation, and all 1,420 configured craters.
  Each surface class is merged into a bounded presentation-only mesh and uses
  deterministic runtime-generated procedural textures for terrain, road, marsh,
  water, ice, crater, rock, structure, bark, leaf, palm, and birch roles.
- Source-plan-driven structure presentation for all 20 maps: 601 road-aligned
  planned buildings, 60 exact tactical landmarks, 197 wall runs, 894 rubble
  piles, 361 sandbag lines, and 346 hedgehogs in five merged material buckets.
  All 62 catalog building kinds now route through explicit C# recipes matching
  their TS identity: framed rural facades, porches, balconies, towers, domes,
  tents, industrial bays, tanks, stacks, gantries, open decks, damaged crowns,
  setback high-rises, arcology bridges, and persistent destructible ownership.
  No production kind falls back to the former profile-only box generator.
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
  bloom and half-resolution depth-normal AO targets, depth-aware AO blur,
  map-fog-driven directional aerial perspective with scope de-hazing, ACES
  output grading, saturation/contrast/vignette, crisp overlay UI, shader-
  missing passthrough, and a reversible AO-trim-resolution-tier quality
  governor that preserves the user's persisted quality choice
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
- Data-driven Garage loadout editor backed by the canonical Unity
  catalog, with 14 era/vehicle-gated equipment choices, three slots, all 112
  match-safe camouflage choices, per-vehicle persistence, live preview paint,
  and selection handoff to solo, private-room, and ranked battles
- Deterministic Unity camouflage textures generated from all 112 canonical
  catalog recipes, including national Factory and per-vehicle Signature
  resolution, vehicle-scale tiling, projected armor UVs, and shared rendering
  across Garage, solo, network battle, Killcam, and archived replay surfaces
- Shared production-fleet running gear with closed discrete-link track meshes,
  road-wheel hubs, sprockets, idlers, return rollers, and inboard suspension
  arms/joints derived from each vehicle's dimensions and track width
- Authored Unity rig alignment for all 126 production vehicles: exact turret
  and gun pivots, measured barrel length/radius, complete armor-plate meshes,
  and catalog-derived visible optics/module parts now drive Garage, solo, and
  network presentation. The eight-vehicle Abrams production family adds its
  bustle/rack, smoke banks, roof hatches, turbine deck grilles, antennas, and
  generation-specific commander stations with camouflage-aware surfaces
- Soviet T-72/T-80/T-90 production-family identity fittings, including
  authored-armor-safe smoke banks, rear drums/logs, T-80 turbine decks,
  searchlight and Shtora distinctions, modern panoramic sights and stowage,
  Ukrainian service kit, and the BMPT T-90 twin-cannon/missile station
- All 11 production Leopard 2 variants share catalog-seated Wegmann smoke
  banks, hatches, MG3, antennas, bustle rack, engine grilles, and optics
  across Garage, solo, and network presentation. Prototype, OTCO, A5NL,
  A4M, A6M, Revolution, A7V, and Ukrainian packages add their distinct
  rangefinders, sensors, remote stations, mine armor, cages, stowage, APU,
  cooling, ADS, and roof equipment without duplicating authored turret or
  side armor
- All six production Challenger 2/3 variants share catalog-seated engine
  grilles, bustle racks, antennas, crew hatches, smoke banks, sights, and
  remote weapon stations across Garage, solo, replay, and network
  presentation. FV4034, 2E, Ukrainian, Challenger 3, and 3 X packages add
  their distinct manual weapons, segmented skirts, fuel drums, cages,
  Trophy sensors, twin autocannons, radar mast, searchlight, bustle cage,
  and field stowage without duplicating catalog-authored ERA or side armor
- All six production Merkava variants share catalog-seated rear clamshell
  service fields, open bustle baskets, ball-and-chain curtains, smoke banks,
  crew hatches, roof weapons, optics, stowage, and radio whips across Garage,
  solo, replay, and network presentation. Mk 1B/2B/2D, Mk 3C/3D, and Mk 4B
  retain distinct mortar, gun-cradle weapon, panoramic sight, roof-load, and
  rear-lattice arrangements without duplicating catalog-authored ERA,
  arrowhead armor, or side armor
- The three production Korean variants share catalog-seated rear service
  fields, engine decks, smoke banks, hatches, antennas, roof optics, weapons,
  and bustle fittings across Garage, solo, replay, and network presentation.
  K1A1 retains its low gunner doghouse, K6/loader weapons, and fully seated
  wrap-around side cages; K2B adds its 16-panel stealth side package, diamond
  turret shell, paired EO heads, twin remote stations, and gun-owned mask
  without duplicating catalog-authored armor or generic side armor
- The six production Japanese variants share catalog-seated rear service
  fields, engine decks, smoke banks, hatches, antennas, optics, weapons, and
  generation-specific bustle fittings across Garage, solo, replay, and network
  presentation. STB-1 and Type 74 retain their cast-generation searchlights
  and open racks; Type 90A adds its visual NERA/service package; Type 10B adds
  paired EO heads, compact RWS, joined Kai basket, and gun-owned mask without
  duplicating its 52 catalog-authored ERA surfaces or generic side armor
- The six production French variants share catalog-seated rear service fields,
  engine decks, hatches, smoke systems, roof weapons, optics, baskets, and
  gun-owned masks across Garage, solo, replay, and network presentation.
  AMX-30/B2 retain their cast cupola, 20 mm coax, PH-8-B and B2 COTAC/LLLTV;
  AMX-40 retains six seated flank panels, three antenna stations, and rear
  rack; Leclerc derivatives retain the HL-70 tower, dual roof weapons, side
  baskets, stowage drum, and variant RWS without duplicating the 72 AMX-30B2
  or 58 AMX 56 catalog ERA surfaces, XLR armor, or generic side armor
- The four production Italian variants share catalog-seated rear service
  fields, engine decks, hatches, smoke systems, antenna stations, roof optics,
  weapons, baskets, and gun-owned masks across Garage, solo, replay, and
  network presentation. Carro 45t retains its asymmetric roof stations,
  crown rails, rear rack, and long 105 mm gun plant; Ariete Preserie retains
  its low M2 fit; C1 retains twin manual MAG stations and turret side racks;
  C2 adds its commander thermal, APU, driver thermal, and remote weapon station
  without duplicating its 100 catalog-authored ERA surfaces or generic side
  armor. National-family dispatch now lives outside the shared generic detail
  builder so additional families do not expand that hub
- The seven production Swedish vehicles retain three distinct presentation
  architectures across Garage, solo, replay, and network. Strv 81 keeps its
  cast-generation optics, Ksp 58, side ventilator, and open basket; Strv 122
  reuses the Leopard 2A5 common package before adding Swedish roof stations
  and basket fittings; UDES 03 and both S-tanks keep every roof, hydraulic,
  fixed-gun, dozer, and nose-fence fitting hull-owned. CV90 and Mk IV own their
  RWS, gun shroud, smoke, optics, and IFV weapon package, removing the generic
  missile pod and adding a twin-cell guided launcher only to Mk IV without
  duplicating catalog armor or generic side armor
- The five production Chinese vehicles preserve their distinct cast, welded,
  and export-turret identities across Garage, solo, replay, and network
  presentation. Type 59 retains its twin cupola weapons, Type 59-II
  searchlight, segmented field package, snorkel and rear rack; ZTZ-85-III
  retains its long welded bustle, side/rear baskets, ISFCS-212, W-85 and radio
  mast; Type 99A retains its twin-row smoke, tall stabilized sights and QJC-88;
  ZTZ-99A2 retains its dense rear service complex; VT-4A1 retains its low
  export roof, warning heads, remote weapon station and deep bustle fittings.
  The modern trio use only their 64/30/41 catalog-authored ERA surfaces and
  never receive generic side armor
- The six production Patton vehicles retain four distinct presentation
  generations across Garage, solo, replay, and network. M46 keeps its low T26
  casting, fender mufflers, M2 and single-baffle M3A1; M47 keeps its long T42
  bustle, paired rangefinders and broad M36 blast deflector; M48A5 keeps its
  asymmetric fender service package, dual M2 stations, diagonal searchlight
  and busy rear rack; M60A1/A3 keep their M19/M85 stations, modernization
  cassettes and distinct searchlight/TTS packages. M60A2 retains its tall
  Starship tower, 152 mm launcher, surface-grid applique, hunter RWS and deep
  lattice bustle. All six use only catalog-authored hit surfaces, including
  M60A3's 66 ERA plates, and add no colliders or generic side armor
- Both production Sheridan variants share catalog-seated engine grilles,
  hatches, eight smoke launchers, two antennas, loader MAG, and a gun-owned
  M81 152 mm launcher package across Garage, solo, replay, and network.
  The base M551 retains its commander M2, rear stowage, two fuel drums, and
  three support rails; M551A1 TTS replaces those field fittings with its rear
  deck extension, skirt cage, protected searchlight, open bustle, electronics,
  and 30 mm remote autocannon. Both variants use only their 38/118
  catalog-authored ERA surfaces and add no colliders or generic side armor
- Both production KF51 variants retain their Leopard-derived hull fittings,
  closed shoulder mudguards, seven-station skirt carriers, eight smoke
  launchers, twin antennas, open-yoke roof weapon and gun-owned Rh-130 package
  across Garage, solo, replay, and network. KF51 keeps its broad closed-chevron
  front, recessed cheek sensors, SEOSS tower, three-rail/eight-brace flank cage
  and five-light RWS; KF51B keeps its convex low crown, seven flank-panel
  stations, raised multispectral sight, upper service grille, bustle cages,
  two-course thermal shroud and three gun cinches. Both use exactly their 60
  catalog hit surfaces, including 44 catalog-authored ERA surfaces, and add no
  colliders, duplicate armor, or generic side armor
- Production Leopard 1A5 now retains its continuous fender shelves, fourteen
  shallow rubber aprons, eight fender lockers, twin rear fuel cans, engine
  intake/fan/louvre deck, B&V cheek package, EMES-18, sixteen Wegmann smoke
  launchers, shielded loader MG3, twin side racks, loaded compact bustle
  basket, and gun-owned butterfly mantlet/L7A3 package across Garage, solo,
  replay, and network. Its 39 catalog-authored hit surfaces remain the only
  armor and it adds no collider or generic side armor
- Production MBT-70 now retains its skirtless M1-derived fenders, exact
  seven-wheel running gear, closed rear shoulders, 8+6 visual applique
  cassettes, broad bustle doors, raised commander/M2 station, gunner optics,
  eight-bank plus paired signature smoke launchers, loaded rear basket, spare
  track racks, twin antennas, and gun-owned five-course parabolic XM150
  launcher package across Garage, solo, replay, and network. Its 29 catalog
  hit surfaces remain authoritative and it adds no collider or generic side
  armor
- Production T-14 Armata now retains its exact seven-wheel stance, three-seat
  hull crew capsule, full-length front-panel/rear-screen skirt architecture,
  engine and stern service fields, low single-crown unmanned turret, six
  recessed cheek channels, ten capped Afganit launchers, distributed
  observation suite, panoramic and meteo masts, paired rear antennas,
  independent 30 mm remote cannon and roof machine gun, and gun-owned clean
  2A82 thermal package across Garage, solo, replay, and network. Its 101
  catalog hit surfaces, including 82 catalog-authored ERA surfaces, remain
  authoritative and it adds no collider or generic side armor
- Production SPz Puma and Puma S1 now retain their exact six-wheel stations,
  raised drive/idler geometry, narrow monocoque hulls, high one-plane bows,
  rear troop ramps, heavy segmented side protection, low unmanned RCT30
  citadels, gun-owned MK30/coax packages, ROSY and sensor suites, and distinct
  launcher fits across Garage, solo, replay, and network. The production Puma
  keeps its pitching twin-round Spike pod and PERI mast; S1 keeps its
  two-layer eight-cassette-per-side AMAP jacket, twin square-cell MELLS,
  four-camera suite, unarmed panoramic yoke, independent compact RWS, and
  open slash-port gun cradle. Their 19/24 catalog hit surfaces remain
  authoritative; catalog track surfaces stay mapped but no longer render over
  the wheels, and neither vehicle adds a collider or generic side armor/pod
- Production M2A2 Bradley, Ukrainian M2A3, and M3A3 CFV now share the exact
  six-wheel donor suspension, narrow continuous tub, closed two-slope bow,
  rear troop ramp, and attached eight-panel skirt/hanger course while keeping
  separate combat identities. M2A2 retains its stepped A2 turret and pitching
  twin TOW pod; the Ukrainian vehicle adds its heavy side/glacis/turret
  package, seated ISU, roof gun, and expanded bustle; M3A3 carries the lower
  welded CFV turret, backed reactive carriers, CIV, paired roof weapons,
  service bins, and deep bustle rack. Their 19/24/117 catalog hit surfaces
  remain authoritative, including all 88 M3A3 ERA surfaces, and none adds a
  collider or generic side armor/pod
- Marder 1A3 now reuses only the Bradley donor hull closure and exact
  six-wheel suspension, replacing the Bradley skirt and fighting compartment
  with its passive eight-panel-per-side A3 applique course, low rounded cast
  turret, external gun carriage, gun-owned MK20 Rh 202 and coaxial MG3. Its
  right-side MILAN, PERI-Z11, ringed commander station, unequal service boxes,
  closed rear equipment wall and basket, roof gun, twin antennas, and paired
  three-round smoke banks remain visible in Garage and battle. All 19 catalog
  hit surfaces remain authoritative, with no added collider or generic side
  armor/pod
- BMP-2 now retains its low closed boat hull, two-plane prow, segmented
  fenders, folded trim vane, seven firing ports with vision blocks, twin
  bulged troop doors, and exact six-wheel front-drive running gear. Its
  centered conical two-man turret carries a roof-owned Konkurs launcher,
  TKN-3 and BPK stations, 3+3 smoke banks, modernization cassettes and warning
  heads, while the 2A42 and coaxial PKT remain gun-owned. The 17 catalog hit
  surfaces stay authoritative across Garage, solo, replay, and network; track
  surfaces remain mapped but hidden over the wheels, with no added collider
  or generic side armor/pod
- BMP-3 and BMP-3 ROK now use a dedicated family owner. The Russian vehicle
  keeps its independent rear-engine boat hull, raked two-plane bow, twin bow
  PKTs, open six-wheel bays, front idler/rear sprocket course, rear troop
  hatches, waterjets, low two-man turret, 3+3 smoke, and gun-owned 2A70,
  2A72, and PKT plant. The ROK vehicle retains the BMP-2 hull lineage while
  adding its 16-panel side course, six glacis panels, supported light
  clusters, taller BMP-3 turret, and 4+4 smoke fit. Each variant preserves
  its 17 catalog hit surfaces across Garage, solo, replay, and network with
  no added collider or generic side armor/pod
- BWP-1 now retains the BMP-2 donor boat hull, firing ports, rear troop doors,
  and exact six-wheel front-drive running gear while adding its own
  16-panel side course, ten glacis panels, supported bow lights, low Polish
  fighting station, twin crew hatches, raised sensor head, rear rack,
  4+4 smoke banks, roof machine gun, twin antennas, and gun-owned MK30.
  Its 17 catalog hit surfaces remain authoritative across Garage, solo,
  replay, and network with no added collider or generic side armor/pod
- Upior now uses its independent compact faceted hull with the corrected
  shackled wedge bow facing forward, twin-door and waterjet stern, crowned
  roof, forward engine deck, 26 shallow skirt panels, and exposed narrow
  six-wheel course with front idler and rear sprocket. Its rear-of-mid
  faceted drum carries twin crew stations, the defining left L-pedestal
  sensor and roof ATGM, 3+3 smoke, roof machine gun, twin antennas, and a
  gun-owned 30 mm/coax plant. All 19 catalog hit surfaces remain
  authoritative across Garage, solo, replay, and network with no added
  collider or generic side armor/pod
- BMPT Terminator 2 now retains the asymmetric T-72B3M hull, rear fuel drums,
  unditching log, six-wheel running gear, and exact offset track course while
  replacing the inherited tank turret with its dedicated aft-shifted
  unmanned turntable. The station carries twin gun-owned 2A42 barrels and
  bores, four separated Ataka tubes, panoramic sight, feed humps, roof
  machine gun, 4+4 smoke, and twin antennas; 14 side and 14 staggered glacis
  protection pieces complete the donor hull. All 153 catalog hit surfaces
  remain authoritative with no added collider or generic side armor/pod,
  while the separate BMPT T-90 Soviet owner remains unchanged
- FV510 Warrior and FV510 Warrior MILAN now share a dedicated low raked hull,
  deep six-module WRAP flank package, open six-wheel front-drive course,
  left exhaust cowl, troop roof and rear door, plus a welded two-man turret
  and gun-owned RARDEN. The MILAN conversion adds six glacis tiles, twelve
  shallow side modules, one live launcher and two bustle tubes without
  replacing the base Warrior. Both retain 19 catalog hit surfaces with no
  added collider or generic side armor/pod
- Mitsubishi Type 89 now uses its dedicated long one-plane glacis, thin open
  skirts over a six-wheel Bradley-shaped course, right-side driver, left
  powerpack and exhaust, six flank firing ports, rear troop door and offset
  welded turret. Twin Type 79 Jyu-MAT boxes, 3+3 smoke and a gun-owned thick
  35 mm KDE with flared vented hider preserve the production identity without
  the previously incorrect roof M2. All 19 catalog hit surfaces remain
  authoritative with no added collider or generic side armor/pod
- Type 89 Light Tiger remains an independent 0.90-scale next-generation
  build rather than a Type 89 skin: its compact planar hull, rear ramp,
  six-wheel front-drive course and eighteen layered side cassettes surround
  a deep-bustle unmanned turret with four Jyu-MAT Kai channels, paired roof
  optics, panoramic station, compact 12.7 mm RWS, 6+6 smoke and gun-owned
  KDE/coax plant. All 24 catalog hit surfaces remain authoritative with no
  added collider or generic side armor/pod
- M1A1 Abrams and M1A1HA now share their source-authored Tejas-family long
  glacis, faceted deep-bustle turret, exact seven-wheel rear-drive course,
  blow-off roof, twin roof weapons and gun-owned M256 plant across Garage,
  solo, replay, and network. M1A1 retains normal-width skirts, binocular
  commander optics, a left-side tow cable and unshielded M2; M1A1HA adds
  widened heavy side modules, one protected commander window, a shielded M2,
  right-side tow cable, three spare track links and a mantlet searchlight.
  Both retain exactly 32 catalog-authored hit surfaces with no added collider
  or generic side armor
- M1A3 Abrams remains a separate first-party next-generation build rather
  than an M1A1/M1A2 skin: its faceted hybrid hull, exact seven-wheel
  rear-drive course, 22 modular skirt cassettes, three-seat forward crew
  capsule, hybrid cooling deck and rear cage support a low unmanned turret
  with isolated six-panel autoloader bustle, modular side protection,
  four-corner APS/radar, distributed sensor towers, four network masts and an
  open-yoke remote weapon station. Its gun-owned segmented 130 mm plant and
  all 69 catalog-authored hit surfaces now render consistently in Garage,
  solo, replay, and network with no added collider or generic Abrams fittings
- AbramsX now uses its independent knife-edge hull, exact seven-wheel
  rear-drive course, 24 kneed skirt cassettes, three-seat forward crew
  capsule, hybrid cooling plenums and asymmetric stern service equipment.
  Its centered low-profile turret carries paired D-hood panoramic heads,
  smoke banks, network masts, and an open XM914 station with visible
  ammunition feed and return links. The gun-owned slim XM360 plant omits the
  bore evacuator and preserves its exact source-visible muzzle station. All
  22 catalog-authored hit surfaces remain authoritative across Garage, solo,
  replay, and network with no added collider or generic Abrams fittings
- The production M1A2 family now shares the source-authored Tejas hull,
  deep-bustle turret, exact seven-wheel rear-drive course, and gun-owned M256
  while replacing the generic Abrams fallback across Garage, solo, replay,
  and network. The base M1A2 keeps its clean standard CROWS and split loader
  shield; TUSK adds the armored CROWS, LAGS loader station, rear slat cage,
  infantry phone, and urban lighting; SEPv2 adds the tall armored CROWS,
  armored loader M2, CIP panels, forward tow cable, rigid ammunition crate,
  and UAAPU exhaust; SEPv3 adds the wide-low CROWS-LP, Trophy launchers and
  four radar faces, left-rear UAAPU, ADL, split IFF, enlarged IFLIR, and
  deterministic physical ULCANS treatment. Catalog armor remains the only
  `Armor-*` owner: the four variants preserve 32/133/129/155 hit surfaces
  (0/101/97/123 reactive) with no presentation collider, generic side armor,
  or generic Abrams fittings
- The legacy `t62mv1` content ID now renders the current source-owned
  T-62 obr. 1975 identity instead of the generic Unity shell. Its widened
  welded hull carries the exact five-wheel course with no return rollers,
  segmented fender bins, exposed tracks, twin rear fuel drums and unditching
  log. The bare organic cast turret carries one DShK, six roof periscopes,
  rear tool rolls and a single radio whip; the gun-owned U-5TS package carries
  the KTD-2 and Luna searchlight through elevation. All 19 catalog hit
  surfaces remain authoritative while their diagnostic plate renderers stay
  hidden behind the dedicated presentation geometry in Garage, solo, replay,
  and network. The vehicle adds no ERA, generic Soviet fittings, side armor,
  return rollers, or presentation colliders
- `t64bv1` now renders its dedicated T-64BV obr. 1985 identity instead of
  the generic Unity shell. The compact hull uses the exact six-small-wheel
  course, four return rollers, raised front idler and rear sprocket, exposed
  upper track, thin segmented skirts, left exhaust, engine louvres, rear log
  and recovery fittings. Its low cast turret carries the raised commander
  gallery, 1G42 and Luna sights, shielded forward NSVT, 902A bank, rear rack,
  short fuel drum and paired OPVT tubes; the segmented 2A46-2 remains owned
  by the authoritative gun articulation. All 117 catalog hit surfaces remain
  authoritative, with all 98 Kontakt-1 surfaces visible and the 19 structural
  diagnostic renderers hidden behind dedicated geometry. Garage, solo,
  replay, and network share this owner and add no generic side armor or
  presentation colliders
- `t72b3m` now has a dedicated T-72B3M obr. 2022 Unity presentation owner
  instead of the shared Soviet fittings layer. The low extended hull carries
  the six-wheel T-72B3M course, Relikt soft-bag skirt run, glacis cassette
  raft, engine louvres, rear slat cage, twin fuel drums and bow fittings.
  Its cast turret adds Relikt cheek cassettes, side soft bags, Sosna-U box,
  commander station, NSVT, bustle slat rack, smoke banks and paired radio
  whips. The 2A46M-5 sleeve/evacuator/muzzle remains attached beneath the
  authoritative `Gun` transform. All 223 catalog hit surfaces remain present,
  with all 204 Relikt ERA surfaces visible and the 19 structural diagnostic
  renderers hidden behind dedicated geometry; no generic side armor, shared
  Soviet fallback fittings, or presentation colliders are added
- `t72bu` now has a dedicated T-72BU / T-90 obr. 1992 Unity presentation
  owner. The port keeps the native six-wheel T-72 running gear, Kontakt-5
  glacis and skirt identity, low cast dome, pointed cheek K-5 wedges,
  Shtora housings, Luna/Agat sight cluster, NSVT, smoke banks, bustle rails,
  rear service drums, unditching log, and the 2A46M-4 fittings under the
  authoritative `Gun` transform. All 180 catalog hit surfaces stay present,
  the 160 ERA renderers stay visible, and the structural diagnostic surfaces
  are hidden behind dedicated geometry without adding generic side armor or
  presentation colliders
- `t90` now has a dedicated T-90 obr. 1992 Unity presentation owner. The
  port keeps the source six-wheel T-90 running gear, Kontakt-5 glacis/skirt
  and cast-dome cheek identity, paired Shtora emitters, 1G46 sight, NSVT,
  smoke banks, bustle rack, rear transom bins, split unditching log, rear
  quarter slat cage, and 2A46M fittings under the authoritative `Gun`
  transform. All 166 catalog hit surfaces stay present, the 141 ERA
  renderers stay visible, and structural diagnostic surfaces are hidden
	  behind dedicated geometry without adding generic side armor or presentation
	  colliders. T-90 now uses the translated C# schema as its only runtime model
	  path. The abandoned TS mesh payload, prefab, imported textures, shader,
	  runtime loader, and editor baker were removed, eliminating roughly 44 MiB
	  of duplicated single-vehicle resources. The first geometry batch ports the
	  TS `t90Gear` envelope values into C# output: rear
	  sprocket, front idler, upper/lower track bands, rising track spans and
	  bottom cleats are now present. The second batch
	  mirrors the TS `t90SkirtCourse` and mudguard anchors: K-5 side-skirt
	  seams, width anchors, forward skirt planes, and front/rear rubber
	  mudguards are now represented in the C# fallback path. The third batch
	  adds a schema extension hook and moves follow-up translated details into
	  `TankT90TranslatedExtras`, covering turret roof optics, cupola periscopes,
	  bustle-rack rails and skirt fasteners without growing the main schema file.
	  The fourth batch adds a reusable C# `MeshPart` helper and begins replacing
	  box-only approximation with translated multi-vertex surfaces for the upper
	  hull wedge, swept glacis, rear deck and turret cheek wedges. The fifth
	  batch extends the translated extras with TS-aligned bustle cargo panels,
	  OPVT base/stay pieces, and cannon-base/gun-mount plates. The sixth batch
	  mirrors the TS `t90-k5-turret-era` seam/cap grammar for the turret roof
	  Kontakt-5 panels and inner/outer cheek leaf banks. The seventh batch ports
	  the geometric part of TS `meshDomeCurved`: C# now has a reusable lathe
	  mesh helper and the T-90 turret dome uses the same ring profile
	  and 30-segment silhouette instead of a Unity sphere primitive. The eighth
	  batch adds the TS-style profile subdivision and cap-normal floor so the
	  C# lathe carries the curved dome normal field instead of faceted default
	  mesh normals. The ninth batch adds a dedicated C# gear-pad translator with
	  156 visible `T90-TrackPad` elements, matching the TS reference's primary
	  `gearTrackPads` count and reducing the slab-like fallback track read. The
	  tenth batch adds a dedicated suspension translator that mirrors the TS
	  road-wheel tire/disc/inset split, 12 suspension links, and 24 joint bosses.
	  The eleventh batch extracts TS-compatible `orientedSlab`, `frustum`,
	  axis-selectable cylinder/cone, and subdivided ring-lathe builders into
	  shared C# shape factories. T-90 now consumes those factories directly;
	  its former local eight-vertex prism helpers and the duplicate mesh/lathe
	  implementation in `TankDetailGeometry` have been removed.
	  The twelfth batch ports the exact TS `box()` size policy onto a shared C#
	  rounded-box builder: dimensions below 0.06 m remain hard-edged, larger
	  parts use one or two bevel segments with a 0.024 m radius cap and the
	  Three.js rounded-face UV projection. The T-90 schema and translated
	  extras now use this path without storing generated mesh assets.
	  The thirteenth batch ports the indexed Three.js torus layout, including
	  normals and UVs, and restores the T-90 bustle cable coil plus both rear
	  tow eyes through the shared C# factory.
	  The fourteenth batch replaces the obsolete rotational dome approximation
	  with the current TS-authoritative eight-station, five-level asymmetric
	  `castSectionLoft`. Its 468 flat-shaded vertices preserve the independent
	  cheek, shoulder, crown, floor, roof, and terminal cap planes.
	  The fifteenth batch ports `polyTurret`, `polyLoft`, and
	  `polyMultiLoft`, including scalar/per-station height and inset values,
	  ring offsets, optional cap heights, outward winding, and zero UVs. The
	  base T-90 now uses the exact 12-point cast-seat and seam lofts instead of
	  the superseded cylindrical turret-ring approximation.
	  The sixteenth batch moves the T-90 running gear off scaled Unity cylinder
	  primitives and onto explicit X-axis C# cylinders. It restores the source
	  `0.385 m` road-wheel radius, `1.395 m` lane center, layered tire/dish/hub
	  widths, source-sized sprocket/idler and return rollers, and torus bow
	  recovery eyes while removing duplicate schema wheel inserts.
	  The seventeenth batch replaces the remaining non-gun Unity cylinder
	  approximations with explicit C# revolution geometry. It restores split
	  log ends and straps, rear fuel drums, layered round Shtora emitters,
	  commander and gunner hatch stacks, complete smoke banks, the OPVT tube,
	  and all five source-authored antenna stations.
	  The eighteenth batch removes the final T-90 schema gun primitives and
	  ports the 2A46M as a gun-local C# assembly: X-axis trunnion saddle,
	  tapered root, elliptical cast collar, three frustum canvas folds, five
	  sleeve rings, four authored tube sections, evacuator swell, and a
	  `5.15 m` visible muzzle independent of the `6 m` combat barrel length.
	  The nineteenth batch removes the superseded `TankT90HullDetails`,
	  `TankT90TurretDetails`, and `TankT90GunDetails` primitive owners. A
	  boundary test now requires every active base-T-90 file to avoid
	  `TankDetailGeometry.Part` and Unity cylinder primitives; TypeScript
	  remains reference-only during translation rather than a bake source.
	  The twentieth batch adds a T-90-only material-role pass after camouflage
	  application. Unity Standard metallic/smoothness and low emission floors
	  now mirror the TS hull, barrel, wheel, rubber, track, detail, dark,
	  glass, wood, and Shtora roles without adding baked texture resources or
	  changing any other vehicle's material path.
  The shared C# shape layer now also includes the TS `loftHull` algorithm:
  merged profile knots, piecewise-linear deck/belly/width/sponson sampling,
  <=0.36 m station subdivision, upper/lower band pinching, and retained
  internal section faces are emitted as one runtime mesh.
  The TS `weldedStationLoft` helper is also available in C#: asymmetric
  three-level cross-sections, sloped side courses, top/bottom planes, and
  station-order-aware end caps are emitted as one flat-shaded runtime mesh.
- `t90a` now has a dedicated T-90A Vladimir Unity presentation owner. The
  port keeps the source six-wheel T-90A course, raised rear sprocket and
  front idler, Kontakt-5 glacis cassettes, rubber skirts with K-5 side
  cassettes, welded/faceted cheek foundation, two-leaf turret K-5 chevron,
  red Shtora emitters, ESSA sight, dual roof cupolas, remote NSVT station,
  rear fuel drums, split unditching log, and 2A46M-2 fittings under the
  authoritative `Gun` transform. All 145 catalog hit surfaces stay
  present, the 105 ERA renderers stay visible, and structural diagnostic
  surfaces are hidden behind dedicated geometry without adding generic side
  armor or presentation colliders. This checkpoint passed focused T-90A
  EditMode coverage, adjacent Soviet fallback coverage, full Unity EditMode
  and PlayMode suites, content and attribution checks, TypeScript typecheck,
  the full `npm test` pre/core/post suite, and public/private production
  builds before its stage commit.
  The first shared-factory conversion batch moves the complete T-90A hull to
  rounded C# boxes plus explicit X/Z-axis cylinders and torus hooks. It
  restores the source `1.395 m` wheel lane, `0.3234 m` painted dish, `0.46 m`
  rear fuel drums with three strap courses, and split 0.85 m unditching logs
  without changing catalog armor or generic suspension ownership.
  The second batch ports the shared T-90SM variable-base turret foundation
  used by the current T-90A source. Its 18-point outline, z=0.50/0.55 lower
  breakpoints, 1.02 flare, 0.78 crown inset, rear shelf, and crown replace the
  former 3.1 m rectangular placeholder at the source seat `(0,1.335,-0.06)`.
  The third batch replaces the remaining Shtora, cupola, lamp, and crosswind
  Unity primitives with source-sized C# box/cylinder assemblies. Both
  three-level cupolas now carry all eight authored periscope blocks, and the
  twelve legacy smoke tubes are removed because the active base `buildT90A`
  source does not create smoke banks.
  The fourth batch ports the complete gun-local 2A46M-2 assembly to shared C#
  shapes: X-axis saddle, tapered root, elliptical cast collar, split recoil
  housing, boot folds, 0.108/0.102 m thermal-jacket courses, fume extractor,
  muzzle collar, and recessed bore. The visible muzzle now follows the source
  `4.92 m` datum independently of the catalog's 6 m combat barrel.
  The fifth batch extends the post-camouflage PBR role pass to `t90a`.
  Hull and barrel retain camouflage, while wheels, tracks, optics, Shtora,
  rubber, wood, and detail fittings recover their TS roughness, metalness,
  dark-olive color floors, and emissive treatment without baked textures.
- `t90a_vladimir` hull migration has started with a dedicated C# owner. Its
  source `-4.755..+2.10 m` longitudinal hull profile now uses the shared loft
  factory, while the six `0.375 m` wheels, `+/-1.46 m` track centers,
  `-3.30/+1.65 m` sprocket/idler stations, return rollers, fenders, skirts,
  and linked courses use variant-specific runtime geometry. The following
  batch replaces the Soviet turret fallback with Vladimir's two-course cast
  shell, crown, frontal/flank K-5, Shtora, ESSA, paired six-tube smoke banks,
  remote Kord, bustle, and source-local 5.32 m 2A46M assembly.
  A dedicated post-camouflage role pass now keeps paint on the hull and gun
  while restoring TS wheel, track, rubber, detail, optic, dark-olive, and
  emissive Shtora materials without generated texture assets.
  The former box bustle is replaced by the source 7-station crown transition
  and 5-station tapered welded bustle, with segmented side rails seated on its
  real shell.
  Hull equipment now includes the source tail drums/rack, layered service
  face and louvres, dark unditching log, five engine grilles, headlight/tow
  fittings, two-row glacis K-5, four-column side K-5 banks, and rear-quarter
  slat cages.
- T-90A Burlak chassis translation has started from the final
  `buildT90BurlakHybridNative2026` owner. It reuses the T-90A body and native
  running gear through dedicated C# entry points, applies the source
  `0.94 x 0.92 x 1.02` hull section and `+0.12 m` seat, narrows the running
  gear gauge to `0.975`, and restores all eight authored fender closures.
  Its generic `Soviet-*` fallback is disabled. The dedicated rotating
  package now includes the final 18-station three-ring core, closed ring
  apron, mirrored shoulder carriers and eight planted K-5 cassettes, plus
  the full-scale five-station welded autoloader bustle with roof lids, side
  pods, rear rails and service grid. Its roof package now includes the
  panoramic head, two hatches, five periscopes, forward sight, autoloader
  feed deck, integrated NSVT station, asymmetric 6/5 smoke banks and the
  2.67 m radio whip. The inherited detailed 2A46M2 assembly now uses the
  Burlak source pivot and `1.318 x 0.955` section/run scaling, with the two
  supplemental barrel courses and five dark sleeve rings. Burlak also uses
  the T-90A hull/barrel/wheel/rubber/track/detail/dark/glass material-role
  pass instead of generic camouflage-only materials.
- Shared TS fitting translation now includes `smokeBank` and `antennaWhip`
  in `TankFittingShapeFactory`. Burlak uses those common builders, including
  all eleven launcher caps, both bank bases, and the antenna base
  pot/collar; existing vehicle-specific fitting callers remain unchanged.
- `TankPintleMachineGunFactory` ports the shared Browning-derived class table
  and complete bearing/spindle/cradle/receiver/feed/barrel/shield load path.
  Burlak's NSVT now uses this shared builder with its source `0.58` scale,
  `-0.075 rad` elevation, connected ammunition feed and standard shield.
  Its final receipt audit also restores both shoulder return faces, station
  head side plates, service box and work light, and reseats all 22 outboard
  skirt/K-5 pieces to the source `x=+/-1.68 m` inner clearance.
- T-90SM translation has started from the final `buildT90SM` owner rather
  than the superseded modern-family proxy. Its source 8/4/4/5-point hull and
  variable track-bay roof now build through `TankHullLoftShapeFactory`, with
  the final belly channels, center keel and rear-deck module attached.
  `TankLinkedTrackShapeFactory` now provides the shared TS half-arc/support
  tangent/equal-pitch track course. T-90SM uses it with its six exact wheel
  stations, `x=+/-1.405 m` lanes, idler/sprocket/return-roller positions,
  contact knees and `0.165 m` pitch; all generic gear renderers are disabled.
  Its dedicated hull-armor owner now adds the two falling bow prongs, 20
  main and two low fender lips, source driver deck/grilles, tow/headlight
  fittings, twelve planted glacis Relikt cassettes, twelve segmented skirt
  panels and the bow caps/flaps/horns. Its stern owner now supplies four
  raked tail racks, corner bins/flaps, nested log/stowage/spare links, the
  twenty-part scalloped curtain, open rear-quarter cage, and 13-louvre rear
  service field. Its rotating package now starts from the source 18-point
  variable-base welded shell, rear casting shelf, crown plate, tapered ring,
  and two raised cupolas at the catalog-compensated TS pivot. The structural
  armor owner adds its broad cheek skin, three-stage nose wedges, flush side
  cassettes/transitions, and asymmetric roof-edge bin. Its roof-equipment
  owner adds the asymmetric bins, Sosna-U, panoramic and backup sights, plus
  the connected armored T05BV-1 station and complete shared-factory NSVT.
  Its squared-bustle owner adds the two rising-underbody cassettes, rear slat
  grille, asymmetric side cells, continuous four-course cage, basket ring,
  and half-sunk OPVT. Its dedicated 2A46M-5 owner replaces the generic gun
  with the source saddle, mantlet plug/canvas/straps, three tube courses,
  six sleeve rings, tapered fume extractor, elliptical muzzle collar and
  recessed bore. The final turret owner computes the six single-row Relikt
  cassettes from the source ring-skin formula and adds their covers/strips,
  rear tower and sight panel, roof sensor, left stowage bin, and both hull
  closure pairs. T-90SM is also routed through the T-90 material applicator:
  painted armor/barrel surfaces retain camouflage while tracks/Relikt,
  optics, rubber and the olive-brown unditching log use their source roles.
  `TankFittingShapeFactory` now also owns the shared centripetal Catmull-Rom
  tube primitive used by its bow and rear-deck tow cables. Whole-vehicle
  receipt and visual audit remains.
- T-90MS translation has started from its distinct final `buildT90MS` owner,
  not the completed T-90SM shell. Its source 10/9/6/6/7-point longitudinal
  hull loft and full closed center glacis now replace the generic hull. Its
  six `0.788 m` cadence road-wheel stations, `dishR=0.72` wheel faces,
  idler/sprocket/roller stations and `0.61 m` linked track course replace all
  generic gear. Its first hull-armor owner adds the 22-piece fender line,
  closed sloped front shoulders/shells, driver deck, five engine grilles,
  glacis fittings, 12 two-course Relikt cassettes with faceplates and flush
  seams, bow cable, log and spare links. Its side/stern owner adds the
  unequal rear drums and tail cells, six tall Relikt skirt cassettes, the
  seven-panel side band, closed crowned front mudguards, five-course flank
  and transom cages, asymmetric service bays and rear fittings.
  `TankMudguardShapeFactory` now carries the shared TS seven-point closed
  crown/cut/rake extrusion and attached support contract. The final source
  rebuild now also owns the rotating pivot, 21-station inner welded shell,
  nine-station continuous outer skin, buried ring and three crown facets.
  Its turret-armor owner adds the two-row/four-carrier frontal chevron,
  48 individually extruded nose tile layers, paired optic heads and all 54
  outer-skin-projected flank/lower/shoulder/roof Relikt parts from the same
  station interpolation and surface-normal equations. Its joined bustle
  continues the crown through a seven-station shoulder into the six-station
  removable magazine, then adds asymmetric service hardware and the complete
  open rear/flank cage. Its roof-equipment owner adds both low crew stations,
  six paired periscopes, the recessed autoloader port, Sosna, the complete
  offset Tagil weapon tower with shared-factory Kord, ten smoke launchers and
  the source-height antenna; no generic Soviet equipment remains. Its
  dedicated 2A46M-5 owner replaces the generic gun with the source trunnion,
  tapered three-section canvas boot, four true-cylinder tube courses, six
  sleeve rings, tapered fume extractor and recessed `5.32 m` bore. T-90MS
  now routes through the T-90 material applicator with its exact final dark,
  rubber, wood, canvas, track, glass and bore-shadow roles. Shared tactical
  markings now use deterministic runtime-generated transparent textures:
  the Russian star and `340` designation occupy the exact surface-solved
  positions/quaternions from `vehicleMarkingSeatGroups/t90.generated.ts`,
  use the final square surface-marking palette from `vehicleMarkings.ts`,
  reverse the decal U axis for Three-to-Unity camera-handedness parity, and
  release both generated textures with the vehicle.
  The final whole-vehicle receipt audit remains a follow-up.
- The same generated-seat marking path now covers every completed T-90
  family vehicle: T-90, T-90A, Vladimir, Burlak, T-90SM and T-90MS. Each
  runtime seat uses the exact per-vehicle position, quaternion and size from
  `vehicleMarkingSeatGroups/t90.generated.ts`; T-90SM preserves both
  surface-solved designation planes plus its separate Russian insignia.
- T-90M and T-90M Proryv now enter their shared native C# owner through the
  final `buildT90MProryvNative2026` pressure-hull path. The extracted
  `TankT90FamilyHullShapeFactory` owns the common T-90 section data, while
  the Proryv hull applies the final installed `-0.04 m` visible offset,
  `1.35 m` flat sponson clearance, central glacis and paired shoulder
  bridges. Its native running gear now supplies the final six `0.31 m`
  road-wheel stations, layered rims/hubs/bolts, four return rollers,
  corrected end-wheel centers and a `.165 m` linked-track course. The final
  hull exterior now includes both authored bow-armor passes, base/lower
  glacis Relikt, driver and engine deck, lights and tow fittings, six-panel
  upper skirts, seven-panel scalloped curtains, shaped mudguards and the
  complete layered stern with drums, log and tow cable. The final welded
  turret core now replaces the generic cylinder with its seven asymmetric
  stations, three buried ring/apron layers, faceted crown, roof saddle and
  broad cheek carriers at the exact installed `(0.95, 0.65, 0.913)` scale.
  Turret protection now preserves the model split: T-90M uses its fourteen
  primary fan modules and six inner-brow modules, while T-90M Proryv uses
  the exact shared Soviet chevron carrier/tile/gasket construction. Both
  retain the four-module-per-side flank course and final surface seams.
  The common aft package now includes the four-station welded magazine,
  service lids and side bins, open cage, backed terminal louvres, attached
  transverse cylinder, armored shoulders and top stores. Equipment and gun
  remain follow-up stages. The dedicated roof-equipment pass now adds both
  crew stations, Sosna, periscopes, the integrated panoramic/Kord station,
  twelve smoke launchers, two antennae and the final searchlight/roof detail
  pass; generic Soviet roof fittings are no longer built. The dedicated
  2A46M-5 now preserves the source pivot and installed counter-scale, sealed
  saddle, accordion boot, ten-stage circular tube, muzzle collar/bore and
  evacuator crest while replacing the generic cube gun. Dedicated material
  roles preserve the source dark/rubber/track/glass and T-90M canvas colors,
  and both variants now consume their generated insignia/designation seats.
  This closes the T-90M/Proryv vehicle pair pending the family-wide audit.
- T-80 family translation has started from the final `buildT80Line` owner.
  T-80, T-80B and T-80BV now share the exact 17/9/2/2/6-station pressure
  hull loft, including the BV-only `1.02 m` rear lower-tub width. Generic
  hull and catalog armor renderers are disabled. Their native running gear
  now adds the six `0.335 m` pressed-wheel stations, torsion arms, five
  return rollers, detailed sprocket/idler faces and the shared `0.165 m`
  linked-shoe course at the source contact and wrap datums. The translated
  hull exterior now owns the turbine shoulders and louvred deck, arrow bow,
  recovery fittings, headlights, mud flaps and seven-panel side-skirt bands;
  the BV keeps its short six-plate K-1 skirt course while the earlier marks
  keep their front returns. The dedicated stern adds the four-rib turbine
  grille, tilted fuel drums, strapped unditching log, bow cable and each
  variant's exact four-link carrier/deck-cable seat. Generic Soviet rear and
  turbine fallback geometry is no longer built for these three vehicles.
  Cast turret, variant armor, gun, materials and markings remain.
- `TankLinkedTrackShapeFactory` now follows the TS end-wheel centers,
  tangent departure/approach angles, ground termination and segmented
  catenary support spans instead of constructing wraps around ground-derived
  proxy centers. Track thickness is explicit and owns the pad height.
- Catalog-authored hydropneumatic aim is authoritative for UDES 03,
  Strv 103/103A, STB-1, Type 74, and MBT-70: the configurable nose-down,
  nose-up, slew-rate, compression, and droop envelopes survive content and
  replay conversion; pitch advances deterministically at 60 Hz,
  toggle through keyboard/gamepad/touch input, drive the shared TankView hull
  pose, survive local prediction and snapshot interpolation, and persist
  through replay v6. UDES 03 and Strv 103/103A also pin gun yaw and
  auto-traverse the hull onto the sight line. Snapshot v5 carries active state
  and pitch while retaining v1-v4 decoding; replay v6 retains v1-v5 decoding
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
- Standalone .NET 8 server host with strict bind/port/origin configuration,
  a bounded 60 Hz service pump, shared Unity-authored C# authority, and
  self-contained Linux/macOS/Windows command-line publishing
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
- C# catalog schema, fleet/map completeness, and runtime-boundary gates

## Remaining parity work

The original release is substantially larger than this first playable port.
These systems still use the TypeScript implementation as their specification:

- complete remaining per-variant exterior fitting parity across the
  126-vehicle production fleet beyond the landed Abrams, Soviet, Leopard 2,
  Challenger 2/3, Merkava, Korean, Japanese, French, and Italian identity
  suites plus the Swedish turreted, siege, and IFV lines;
- high-end shader finish parity for map materials/vegetation, alpha-card
  foliage and broader world streaming for all 20 maps;
- remaining production UI polish;
- installable build-target release artifacts and platform packaging;
- per-family procedural vehicle geometry parity and generated technical assets.

Migrate these by extending the simulation contracts rather than moving
authority into MonoBehaviours or PhysX. The TypeScript project should remain
buildable until each replacement has parity tests.
