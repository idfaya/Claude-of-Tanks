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

- complete remaining per-variant exterior fitting parity across the
  126-vehicle production fleet beyond the landed Abrams, Soviet, Leopard 2,
  Challenger 2/3, Merkava, Korean, Japanese, French, and Italian identity
  suites plus the Swedish turreted, siege, and IFV lines;
- per-family structure geometry/material parity, complete vegetation recipes,
  and broader world streaming for all 20 maps;
- remaining production UI polish;
- installable build-target release artifacts, WebRTC signaling/private-room
  session composition, and signaling deployment;
- per-family procedural vehicle geometry parity and generated technical assets.

Migrate these by extending the simulation contracts rather than moving
authority into MonoBehaviours or PhysX. The TypeScript project should remain
buildable until each replacement has parity tests.
