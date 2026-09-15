using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed partial class MatchModeSimulation
    {
        private void StepHorde()
        {
            if (!_hordeStarted)
            {
                _hordeStarted = true;
                StartHordeWave();
            }
            if (!HasAlive(Team.Alpha))
            {
                _state.Winner = Team.Bravo;
                return;
            }
            int alive = CountActive(
                Team.Bravo);
            _state.HordeAlive = alive;
            _state.HordeNextWaveInS =
                _nextHordeWaveAtS < 0f
                    ? 0f
                    : MathF.Max(
                        0f,
                        _nextHordeWaveAtS -
                        _battle.TimeS);
            if (alive == 0 &&
                _nextHordeWaveAtS < 0f)
            {
                _nextHordeWaveAtS =
                    _battle.TimeS +
                    HordeIntermissionS;
                _state.HordeNextWaveInS =
                    HordeIntermissionS;
                SpawnPickup();
            }
            if (_nextHordeWaveAtS >= 0f &&
                _battle.TimeS >=
                    _nextHordeWaveAtS)
            {
                _state.HordeWave++;
                StartHordeWave();
            }
            CollectPickups();
        }

        private void StartHordeWave()
        {
            int enemies = 0;
            for (int i = 0;
                i < _battle.Tanks.Count;
                i++)
            {
                if (_battle.Tanks[i].Team ==
                    Team.Bravo)
                {
                    enemies++;
                }
            }
            int active = Math.Min(
                enemies,
                HordeInitialActive +
                (_state.HordeWave - 1) / 2);
            float healthScale =
                1f +
                (_state.HordeWave - 1) *
                0.16f;
            int enemyIndex = 0;
            for (int i = 0;
                i < _battle.Tanks.Count;
                i++)
            {
                TankState tank =
                    _battle.Tanks[i];
                if (tank.Team ==
                    Team.Alpha)
                {
                    if (tank.Destroyed)
                        Respawn(tank);
                    continue;
                }
                bool enabled =
                    enemyIndex++ < active;
                tank.ModeActive = enabled;
                if (!enabled)
                {
                    tank.Destroyed = true;
                    tank.Combat.Destroyed = true;
                    tank.SpeedMps = 0f;
                    continue;
                }
                Respawn(tank);
                tank.ModeSpeedMultiplier =
                    1f +
                    MathF.Min(
                        0.55f,
                        (_state.HordeWave -
                         1) * 0.045f);
                tank.Combat.MaxHealth =
                    tank.DamageSpec.MaxHealth *
                    healthScale;
                tank.Combat.Health =
                    tank.Combat.MaxHealth;
                tank.Health =
                    tank.Combat.Health;
            }
            _state.HordeTotal = active;
            _state.HordeAlive = active;
            _state.HordeNextWaveInS = 0f;
            _nextHordeWaveAtS = -1f;
        }

        private void SpawnPickup()
        {
            bool heal =
                _battle.Random.NextFloat() <
                MathF.Max(
                    0.08f,
                    0.62f -
                    (_state.HordeWave - 1) *
                    0.055f);
            float extent =
                MathF.Min(
                    135f,
                    _battle.WorldHalfExtentM *
                    0.4f);
            Float3 middle =
                (_alphaBase + _bravoBase) *
                0.5f;
            float x = middle.X +
                _battle.Random.Range(
                    -extent,
                    extent);
            float z = middle.Z +
                _battle.Random.Range(
                    -extent,
                    extent);
            _state.Pickups.Add(
                new ModePickup
                {
                    Id = "pickup-" +
                        _pickupSequence++,
                    Kind = heal
                        ? "heal"
                        : "ammo",
                    Position = new Float3(
                        x,
                        _battle.HeightField
                            .HeightAt(x, z) +
                            0.5f,
                        z),
                    Active = true,
                    SpawnedWave =
                        _state.HordeWave
                });
        }

        private void CollectPickups()
        {
            for (int pickupIndex = 0;
                pickupIndex <
                    _state.Pickups.Count;
                pickupIndex++)
            {
                ModePickup pickup =
                    _state.Pickups[
                        pickupIndex];
                if (!pickup.Active)
                    continue;
                for (int tankIndex = 0;
                    tankIndex <
                        _battle.Tanks.Count;
                    tankIndex++)
                {
                    TankState tank =
                        _battle.Tanks[
                            tankIndex];
                    if (tank.Team !=
                            Team.Alpha ||
                        tank.Destroyed ||
                        HorizontalDistanceSq(
                            tank.Position,
                            pickup.Position) >
                        PickupRadiusM *
                        PickupRadiusM)
                    {
                        continue;
                    }
                    if (pickup.Kind == "heal")
                    {
                        tank.Combat.Health =
                            MathF.Min(
                                tank.Combat
                                    .MaxHealth,
                                tank.Combat.Health +
                                tank.Combat
                                    .MaxHealth *
                                0.35f);
                        tank.Health =
                            tank.Combat.Health;
                    }
                    else
                    {
                        for (int slot = 0;
                            slot <
                                tank.Combat
                                    .Ammo.Length;
                            slot++)
                        {
                            tank.Combat.Ammo[slot] =
                                tank.Combat
                                    .AmmoCapacity[
                                        slot];
                        }
                    }
                    pickup.Active = false;
                    break;
                }
            }
        }

        private int CountActive(Team team)
        {
            int count = 0;
            for (int i = 0;
                i < _battle.Tanks.Count;
                i++)
            {
                TankState tank =
                    _battle.Tanks[i];
                if (tank.Team == team &&
                    tank.ModeActive &&
                    !tank.Destroyed)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
