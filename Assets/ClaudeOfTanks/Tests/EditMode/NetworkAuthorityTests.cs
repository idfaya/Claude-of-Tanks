using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class NetworkAuthorityTests
    {
        [Test]
        public void HostRejectsStaleAndFutureInputThenAdvancesAuthority()
        {
            BattleState state = new BattleState(new FlatHeightField(), 91u);
            TankState player = Tank("entity-alpha", Team.Alpha, Float3.Zero, 0f);
            state.Tanks.Add(player);
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer-alpha", player.Id);
            int initialAmmo = player.Combat.Ammo[0];
            NetworkInputCommand command = Command("peer-alpha", 0u, 0);
            command.Throttle = 1f;
            command.Actions = NetworkActionBits.Fire;
            command.ActionSequence = 7u;

            Assert.That(host.SubmitInput(command), Is.EqualTo(InputAdmission.Accepted));
            Assert.That(host.SubmitInput(command), Is.EqualTo(InputAdmission.Stale));

            NetworkInputCommand future = Command(
                "peer-alpha", 1u, NetworkProtocol.MaximumFutureTicks + 1);
            Assert.That(host.SubmitInput(future), Is.EqualTo(InputAdmission.TooFarAhead));
            Assert.That(host.AdvanceTicks(20), Is.EqualTo(AuthoritativeMatchHost.MaximumCatchUpTicks));
            Assert.That(host.Tick, Is.EqualTo(AuthoritativeMatchHost.MaximumCatchUpTicks));
            Assert.That(player.Position.Z, Is.GreaterThan(0f));
            Assert.That(player.Combat.Ammo[0], Is.EqualTo(initialAmmo - 1));

            NetworkWorldSnapshot snapshot = host.CreateSnapshot("peer-alpha");
            Assert.That(snapshot.AcknowledgedInputSequence, Is.EqualTo(0u));
            Assert.That(snapshot.Tick, Is.EqualTo(host.Tick));
        }

        [Test]
        public void SnapshotFiltersHiddenEnemiesBeforeSerialization()
        {
            BattleState state = new BattleState(new FlatHeightField(), 92u);
            TankState viewer = Tank("entity-viewer", Team.Alpha, Float3.Zero, 0f);
            TankState ally = Tank("entity-ally", Team.Alpha, new Float3(30f, 0f, 0f), 0f);
            TankState hidden = Tank("entity-hidden", Team.Bravo, new Float3(0f, 0f, -100f), 0f);
            state.Tanks.Add(viewer);
            state.Tanks.Add(ally);
            state.Tanks.Add(hidden);
            state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ShellFired,
                SourceId = hidden.Id,
                Position = hidden.Position
            });
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer-viewer", viewer.Id);
            host.RegisterPlayer(
                "peer-hidden",
                hidden.Id);
            host.RegisterSpectator("observer");

            NetworkWorldSnapshot filtered = host.CreateSnapshot("peer-viewer");
            Assert.That(filtered.Entities, Has.Length.EqualTo(2));
            Assert.That(ContainsEntity(filtered, hidden.Id), Is.False);
            Assert.That(filtered.Events, Is.Empty);

            NetworkWorldSnapshot observer = host.CreateSnapshot("observer");
            Assert.That(observer.Entities, Has.Length.EqualTo(3));
            Assert.That(ContainsEntity(observer, hidden.Id), Is.True);
            Assert.That(observer.Events, Has.Length.EqualTo(1));

            hidden.Position = new Float3(0f, 0f, 100f);
            hidden.Yaw = MathUtil.Pi;
            host.AdvanceTicks(1);
            filtered = host.CreateSnapshot("peer-viewer");
            Assert.That(ContainsEntity(filtered, hidden.Id), Is.True);
            Assert.That(filtered.ViewerSpotted, Is.False);
            for (int i = 0; i < 190; i++)
                host.AdvanceTicks(1);
            filtered = host.CreateSnapshot("peer-viewer");
            Assert.That(filtered.ViewerSpotted, Is.True);
        }

        [Test]
        public void SnapshotDoesNotSerializeEnemyBehindStaticStructure()
        {
            BattleState state = new BattleState(
                new FlatHeightField(),
                94u,
                500f,
                new[]
                {
                    new StaticObstacle(
                        "warehouse",
                        new Float3(0f, 0f, 60f),
                        12f,
                        8f,
                        10f,
                        0f,
                        StaticObstacleFlags.All)
                });
            TankState viewer = Tank("entity-viewer", Team.Alpha, Float3.Zero, 0f);
            TankState hidden = Tank(
                "entity-hidden", Team.Bravo, new Float3(0f, 0f, 120f), MathUtil.Pi);
            state.Tanks.Add(viewer);
            state.Tanks.Add(hidden);
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer-viewer", viewer.Id);

            NetworkWorldSnapshot snapshot = host.CreateSnapshot("peer-viewer");

            Assert.That(ContainsEntity(snapshot, hidden.Id), Is.False);
        }

        [Test]
        public void StructureDestructionStateIsIdenticalForEveryViewer()
        {
            BattleState state = new BattleState(
                new FlatHeightField(),
                95u,
                500f,
                new[]
                {
                    new StaticObstacle(
                        "global-cover",
                        new Float3(0f, 0f, 60f),
                        4f,
                        4f,
                        5f,
                        0f,
                        StaticObstacleFlags.All,
                        true)
                });
            TankState alpha = Tank("entity-alpha", Team.Alpha, Float3.Zero, 0f);
            TankState bravo = Tank(
                "entity-bravo", Team.Bravo, new Float3(0f, 0f, 120f), MathUtil.Pi);
            state.Tanks.Add(alpha);
            state.Tanks.Add(bravo);
            state.DamageStaticObstacle(0, 10000f);
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer-alpha", alpha.Id);
            host.RegisterPlayer("peer-bravo", bravo.Id);

            NetworkWorldSnapshot alphaSnapshot = host.CreateSnapshot("peer-alpha");
            NetworkWorldSnapshot bravoSnapshot = host.CreateSnapshot("peer-bravo");

            Assert.That(alphaSnapshot.StaticObstacleRevision, Is.EqualTo(1u));
            Assert.That(bravoSnapshot.StaticObstacleRevision, Is.EqualTo(1u));
            Assert.That(
                alphaSnapshot.DestroyedStaticObstacleIndices,
                Is.EqualTo(new ushort[] { 0 }));
            Assert.That(
                bravoSnapshot.DestroyedStaticObstacleIndices,
                Is.EqualTo(new ushort[] { 0 }));
        }

        [Test]
        public void EntityIdentityDoesNotAliasDuplicateVehicleSelections()
        {
            BattleState state = new BattleState(new FlatHeightField(), 93u);
            TankSpec sharedVehicle = TankSpec.Medium();
            state.Tanks.Add(new TankState(
                "commander-one", Team.Alpha, sharedVehicle, new Float3(-5f, 0f, 0f), 0f));
            state.Tanks.Add(new TankState(
                "commander-two", Team.Alpha, sharedVehicle, new Float3(5f, 0f, 0f), 0f));
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer-one", "commander-one");
            host.RegisterPlayer("peer-two", "commander-two");

            NetworkWorldSnapshot snapshot = host.CreateSnapshot("peer-one");

            Assert.That(snapshot.Entities, Has.Length.EqualTo(2));
            Assert.That(snapshot.Entities[0].EntityId, Is.Not.EqualTo(snapshot.Entities[1].EntityId));
            Assert.That(snapshot.Entities[0].VehicleSpecId, Is.EqualTo(snapshot.Entities[1].VehicleSpecId));
        }

        [Test]
        public void SequenceComparisonSupportsUnsignedWraparound()
        {
            Assert.That(NetworkProtocol.IsSequenceNewer(0u, uint.MaxValue), Is.True);
            Assert.That(NetworkProtocol.IsSequenceNewer(uint.MaxValue, 0u), Is.False);
        }

        [Test]
        public void SnapshotCarriesAuthoritativeModuleAndCrewState()
        {
            BattleState state = new BattleState(new FlatHeightField(), 97u);
            TankState player = Tank(
                "entity-alpha",
                Team.Alpha,
                Float3.Zero,
                0f);
            state.Tanks.Add(player);
            DamageSimulation.DamageModule(
                player.Combat,
                "engine",
                10000f,
                () => 1f);
            DamageSimulation.KnockOutCrew(player.Combat, "gunner");
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer-alpha", player.Id);

            NetworkEntitySnapshot entity =
                host.CreateSnapshot("peer-alpha").Entities[0];
            DamageCombatState restored =
                DamageSimulation.CreateCombatState(player.DamageSpec);
            NetworkDamageState.Apply(restored, entity);

            Assert.That(
                restored.Modules["engine"].Condition,
                Is.EqualTo(DamageModuleCondition.Red));
            Assert.That(restored.Crew["commander"], Is.True);
            Assert.That(restored.Crew["gunner"], Is.False);
        }

        [Test]
        public void TeamSpottingSharesAndLingersBeforeExpiry()
        {
            BattleState state =
                new BattleState(
                    new FlatHeightField(),
                    98u);
            TankState viewer = Tank(
                "viewer",
                Team.Alpha,
                Float3.Zero,
                MathUtil.Pi);
            TankState scout = Tank(
                "scout",
                Team.Alpha,
                new Float3(20f, 0f, 0f),
                0f);
            TankState target = Tank(
                "target",
                Team.Bravo,
                new Float3(20f, 0f, 100f),
                MathUtil.Pi);
            state.Tanks.Add(viewer);
            state.Tanks.Add(scout);
            state.Tanks.Add(target);
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(
                    new BattleSimulation(state));
            host.RegisterPlayer("viewer-peer", viewer.Id);
            host.RegisterPlayer("scout-peer", scout.Id);
            host.RegisterPlayer("target-peer", target.Id);

            host.AdvanceTicks(1);
            Assert.That(
                ContainsEntity(
                    host.CreateSnapshot(
                        "viewer-peer"),
                    target.Id),
                Is.True);

            target.Position =
                new Float3(100f, 0f, 0f);
            host.AdvanceTicks(1);
            Assert.That(
                ContainsEntity(
                    host.CreateSnapshot(
                        "viewer-peer"),
                    target.Id),
                Is.True);

            for (int i = 0; i < 310; i++)
                host.AdvanceTicks(1);
            Assert.That(
                ContainsEntity(
                    host.CreateSnapshot(
                        "viewer-peer"),
                    target.Id),
                Is.False);
        }

        private static NetworkInputCommand Command(
            string playerId,
            uint sequence,
            long clientTick)
        {
            return new NetworkInputCommand
            {
                PlayerId = playerId,
                Sequence = sequence,
                ClientTick = clientTick,
                AimYawRad = 0f,
                AimPitchRad = 0f,
                AimDistanceM = 100f
            };
        }

        private static TankState Tank(string id, Team team, Float3 position, float yaw)
        {
            return new TankState(id, team, TankSpec.Medium(), position, yaw);
        }

        private static bool ContainsEntity(NetworkWorldSnapshot snapshot, string entityId)
        {
            for (int i = 0; i < snapshot.Entities.Length; i++)
                if (snapshot.Entities[i].EntityId == entityId) return true;
            return false;
        }
    }
}
