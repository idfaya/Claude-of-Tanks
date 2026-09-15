using System;

namespace ClaudeOfTanks.Simulation
{
    public static class TankMovement
    {
        private const float DriveAccelerationPerHpPerTon = 0.16f;
        private const float CoastDecelerationMps2 = 2.4f;
        private const float BrakeDecelerationMps2 = 9f;
        private const float BloomGrowTimeS = 0.05f;
        private const float AimSettledRatio = 6f;
        private const float GravityMps2 = 9.81f;
        private const float GroundSnapDistanceM = 0.35f;
        private const float AttitudeResponseS = 0.12f;
        private const float RolloverAngleRad =
            75f * MathUtil.Deg2Rad;
        private const float RolloverDelayS = 1f;
        private const float CasemateSteerRampRad =
            8f * MathUtil.Deg2Rad;

        public static void Step(TankState tank, TankInput input, IHeightField heightField, float dt)
        {
            if (tank.Destroyed || dt <= 0f)
            {
                return;
            }

            float previousYaw = tank.Yaw;
            float previousTurretYaw = tank.TurretYaw;
            float throttle = MathUtil.Clamp(input.Throttle, -1f, 1f);
            float steer = MathUtil.Clamp(input.Steer, -1f, 1f);
            steer = HydropneumaticAimSimulation.ResolveSteer(
                tank,
                input,
                steer);
            steer = ResolveCasemateSteer(
                tank,
                input.AimPoint,
                steer);
            float mobility = MobilityMultiplier(tank);
            float steering = SteeringMultiplier(tank);
            float forwardLimit =
                tank.Spec.TopSpeedKmh /
                3.6f * mobility *
                MathF.Max(
                    0f,
                    tank.ModeSpeedMultiplier);
            float reverseLimit =
                tank.Spec.ReverseSpeedKmh /
                3.6f * mobility *
                MathF.Max(
                    0f,
                    tank.ModeSpeedMultiplier);
            float targetSpeed = throttle >= 0f ? throttle * forwardLimit : throttle * reverseLimit;
            float powerToWeight = tank.Spec.EnginePowerHp / MathF.Max(1f, tank.Spec.WeightTons);
            ITerrainSurface surface = heightField as ITerrainSurface;
            Float3 groundNormal = surface != null
                ? surface.NormalAt(tank.Position.X, tank.Position.Z).Normalized
                : new Float3(0f, 1f, 0f);
            float surfaceResistance = surface != null
                ? MathF.Max(0.25f, surface.ResistanceAt(tank.Position.X, tank.Position.Z))
                : 1f;
            float slopeTraction = MathUtil.Clamp01(groundNormal.Y * MathF.Max(0.1f, tank.Spec.TrackTraction));
            float acceleration = MathUtil.Clamp(
                powerToWeight * DriveAccelerationPerHpPerTon,
                1.8f,
                7.5f) * mobility * slopeTraction /
                MathF.Max(
                    0.25f,
                    tank.Spec.TerrainResistance *
                    surfaceResistance);

            if (input.Brake)
            {
                tank.SpeedMps = MathUtil.MoveTowards(tank.SpeedMps, 0f, BrakeDecelerationMps2 * dt);
            }
            else if (MathF.Abs(throttle) > 0.01f)
            {
                tank.SpeedMps = MathUtil.MoveTowards(tank.SpeedMps, targetSpeed, acceleration * dt);
            }
            else
            {
                tank.SpeedMps = MathUtil.MoveTowards(tank.SpeedMps, 0f, CoastDecelerationMps2 * dt);
            }

            float speedFraction = MathUtil.Clamp01(MathF.Abs(tank.SpeedMps) / MathF.Max(1f, forwardLimit));
            float pivotFactor = MathF.Abs(tank.SpeedMps) < 0.3f ? 0.72f : 1f - speedFraction * 0.35f;
            float direction = tank.SpeedMps < -0.05f ? -1f : 1f;
            tank.Yaw += steer * direction * tank.Spec.HullTraverseDegS *
                tank.TraverseMultiplier * steering *
                MathUtil.Deg2Rad * pivotFactor * dt;

            Float3 forward = Float3.Forward(tank.Yaw);
            Float3 next = tank.Position + forward * (tank.SpeedMps * dt);
            UpdateVerticalAndAttitude(
                tank,
                heightField,
                ref next,
                dt);
            tank.Position = next;
            HydropneumaticAimSimulation.Step(
                tank,
                input,
                dt);

            Float3 toAim = input.AimPoint - tank.Position;
            if (tank.Spec.FixedHydraulicGun)
            {
                tank.TurretYaw = 0f;
                tank.GunPitchRad = 0f;
            }
            else if (toAim.X * toAim.X + toAim.Z * toAim.Z > 0.001f)
            {
                float desiredWorldYaw = MathF.Atan2(toAim.X, toAim.Z);
                float desiredLocalYaw = MathUtil.DeltaAngle(tank.Yaw, desiredWorldYaw);
                float gunArc =
                    tank.Spec.GunArcDeg *
                    MathUtil.Deg2Rad;
                bool limitedArc =
                    tank.Spec.Armor?.Turretless ==
                        true ||
                    gunArc < MathUtil.Pi -
                        0.0001f;
                if (limitedArc)
                {
                    desiredLocalYaw =
                        MathUtil.Clamp(
                            desiredLocalYaw,
                            -gunArc,
                            gunArc);
                }
                float delta = MathUtil.DeltaAngle(tank.TurretYaw, desiredLocalYaw);
                float maxStep = tank.Spec.TurretTraverseDegS *
                    tank.TurretMultiplier *
                    CrewAlive(tank, "gunner", 1f, 0.5f) *
                    MathUtil.Deg2Rad * dt;
                tank.TurretYaw += MathUtil.Clamp(delta, -maxStep, maxStep);
                if (limitedArc)
                {
                    tank.TurretYaw =
                        MathUtil.Clamp(
                            tank.TurretYaw,
                            -gunArc,
                            gunArc);
                }
            }
            UpdateGunPitch(tank, input.AimPoint, dt);

            tank.HullYawRateRadS =
                MathUtil.DeltaAngle(previousYaw, tank.Yaw) / dt;
            tank.TurretYawRateRadS =
                MathUtil.DeltaAngle(previousTurretYaw, tank.TurretYaw) / dt;
            UpdateAimBloom(tank, dt);
        }

        private static void UpdateVerticalAndAttitude(
            TankState tank,
            IHeightField heightField,
            ref Float3 next,
            float dt)
        {
            float groundY =
                heightField.HeightAt(next.X, next.Z);
            float drop =
                tank.Position.Y - groundY;
            if (tank.Grounded &&
                drop <= GroundSnapDistanceM)
            {
                next.Y = groundY;
                tank.VerticalSpeedMps = 0f;
            }
            else
            {
                tank.Grounded = false;
                tank.VerticalSpeedMps -=
                    GravityMps2 * dt;
                next.Y = tank.Position.Y +
                    tank.VerticalSpeedMps * dt;
                if (next.Y <= groundY)
                {
                    next.Y = groundY;
                    tank.VerticalSpeedMps = 0f;
                    tank.Grounded = true;
                }
            }
            if (groundY >
                tank.Position.Y +
                GroundSnapDistanceM)
            {
                next.Y = groundY;
                tank.VerticalSpeedMps = 0f;
                tank.Grounded = true;
            }

            Float3 normal = TerrainNormal(
                heightField,
                next.X,
                next.Z);
            Float3 forward =
                Float3.Forward(tank.Yaw);
            Float3 right = new Float3(
                MathF.Cos(tank.Yaw),
                0f,
                -MathF.Sin(tank.Yaw));
            float targetPitch = tank.Grounded
                ? MathF.Atan2(
                    -Float3.Dot(normal, forward),
                    MathF.Max(0.0001f, normal.Y))
                : 0f;
            float targetRoll = tank.Grounded
                ? MathF.Atan2(
                    Float3.Dot(normal, right),
                    MathF.Max(0.0001f, normal.Y))
                : 0f;
            float blend =
                1f -
                MathF.Exp(
                    -dt /
                    AttitudeResponseS);
            tank.TerrainPitchRad +=
                (targetPitch -
                 tank.TerrainPitchRad) *
                blend;
            tank.HullRollRad +=
                (targetRoll -
                 tank.HullRollRad) *
                blend;

            bool unstable =
                MathF.Abs(tank.HullRollRad) >=
                    RolloverAngleRad ||
                MathF.Abs(
                    tank.TerrainPitchRad +
                    tank.HullPitchRad) >=
                    RolloverAngleRad;
            tank.RolloverTimerS = unstable
                ? tank.RolloverTimerS + dt
                : 0f;
            if (tank.RolloverTimerS >=
                RolloverDelayS)
            {
                tank.Overturned = true;
                tank.SpeedMps = 0f;
            }
        }

        private static Float3 TerrainNormal(
            IHeightField heightField,
            float x,
            float z)
        {
            ITerrainSurface surface =
                heightField as ITerrainSurface;
            if (surface != null)
                return surface.NormalAt(x, z)
                    .Normalized;
            const float sample = 0.5f;
            float dx =
                heightField.HeightAt(
                    x - sample,
                    z) -
                heightField.HeightAt(
                    x + sample,
                    z);
            float dz =
                heightField.HeightAt(
                    x,
                    z - sample) -
                heightField.HeightAt(
                    x,
                    z + sample);
            return new Float3(
                dx,
                sample * 2f,
                dz).Normalized;
        }

        private static float ResolveCasemateSteer(
            TankState tank,
            Float3 aimPoint,
            float steer)
        {
            if (tank.Spec.FixedHydraulicGun ||
                tank.Spec.Armor?.Turretless !=
                    true)
            {
                return steer;
            }
            Float3 toAim =
                aimPoint - tank.Position;
            if (toAim.X * toAim.X +
                    toAim.Z * toAim.Z <=
                0.001f)
            {
                return steer;
            }
            float desiredYaw =
                MathF.Atan2(
                    toAim.X,
                    toAim.Z);
            float local = MathUtil.DeltaAngle(
                tank.Yaw,
                desiredYaw);
            float arc =
                tank.Spec.GunArcDeg *
                MathUtil.Deg2Rad;
            float excess =
                MathF.Abs(local) - arc;
            if (excess <= 0f) return steer;
            float automatic =
                MathF.Sign(local) *
                MathUtil.Clamp01(
                    excess /
                    CasemateSteerRampRad);
            return MathF.Abs(steer) >
                MathF.Abs(automatic)
                    ? steer
                    : automatic;
        }

        private static void UpdateGunPitch(
            TankState tank,
            Float3 aimPoint,
            float dt)
        {
            if (tank.Spec.FixedHydraulicGun)
                return;
            Float3 gunPivot =
                tank.Spec.Armor != null
                    ? tank.Spec.Armor.GunPivot
                    : new Float3(
                        0f,
                        1.5f,
                        0f);
            Float3 toAim =
                aimPoint -
                (tank.Position + gunPivot);
            float horizontal =
                MathF.Sqrt(
                    toAim.X * toAim.X +
                    toAim.Z * toAim.Z);
            if (horizontal <= 0.001f)
                return;
            float desiredWorld =
                MathF.Atan2(
                    toAim.Y,
                    horizontal);
            float desiredLocal =
                desiredWorld -
                tank.TerrainPitchRad -
                tank.HullPitchRad;
            float low =
                -tank.Spec.GunDepressionDeg *
                MathUtil.Deg2Rad;
            float high =
                tank.Spec.GunElevationDeg *
                MathUtil.Deg2Rad;
            tank.AtGunLimit =
                desiredLocal < low ||
                desiredLocal > high;
            float target =
                MathUtil.Clamp(
                    desiredLocal,
                    low,
                    high);
            float step =
                tank.Spec.GunPitchDegS *
                MathUtil.Deg2Rad * dt;
            tank.GunPitchRad =
                MathUtil.MoveTowards(
                    tank.GunPitchRad,
                    target,
                    step);
        }

        public static float DispersionSigmaRad(TankState tank)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            return MathF.Max(0f, tank.Spec.BaseAccuracyMAt100) *
                MathF.Max(1f, tank.AimBloom) / 200f;
        }

        public static void ApplyPostShotBloom(TankState tank)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            tank.AimBloom = MathF.Max(
                1f,
                tank.AimBloom *
                MathF.Max(1f, tank.Spec.AimBloomAfterShot));
        }

        private static void UpdateAimBloom(TankState tank, float dt)
        {
            TankSpec spec = tank.Spec;
            float move = spec.AimBloomMove *
                MathF.Abs(tank.SpeedMps) * 3.6f;
            float hull = spec.AimBloomHullRotation *
                MathF.Abs(tank.HullYawRateRadS) / MathUtil.Deg2Rad;
            float turret = spec.AimBloomTurretRotation *
                MathF.Abs(tank.TurretYawRateRadS) / MathUtil.Deg2Rad;
            float target = MathF.Sqrt(
                1f + move * move + hull * hull + turret * turret);
            DamageModuleState gun;
            if (tank.Combat.Modules.TryGetValue("gun", out gun) &&
                gun.Condition == DamageModuleCondition.Yellow)
            {
                target *= 2f;
            }
            target *= CrewAlive(tank, "gunner", 1f, 1.5f);
            float bloomMultiplier = MathF.Max(
                0f,
                tank.Combat.Equipment.Bloom);
            target = 1f + (target - 1f) * bloomMultiplier;

            float aimTime = MathF.Max(
                0.01f,
                spec.AimTimeS *
                MathF.Max(0.01f, tank.Combat.Equipment.AimTime));
            float tau = target > tank.AimBloom
                ? BloomGrowTimeS
                : aimTime / MathF.Log(AimSettledRatio);
            float blend = 1f - MathF.Exp(-dt / tau);
            tank.AimBloom += (target - tank.AimBloom) * blend;
            if (tank.AimBloom < 1f) tank.AimBloom = 1f;
        }

        private static float MobilityMultiplier(TankState tank)
        {
            float value = ModuleMultiplier(
                tank,
                "engine",
                0.65f,
                0.25f);
            value *= ModuleMultiplier(
                tank,
                "transmission",
                0.75f,
                0.4f);
            value *= CrewAlive(tank, "driver", 1f, 0.55f);
            bool leftRed = IsRed(tank, "trackL");
            bool rightRed = IsRed(tank, "trackR");
            if (leftRed && rightRed) return 0f;
            if (leftRed || rightRed) value *= 0.25f;
            return value;
        }

        private static float SteeringMultiplier(TankState tank)
        {
            bool leftRed = IsRed(tank, "trackL");
            bool rightRed = IsRed(tank, "trackR");
            if (leftRed && rightRed) return 0f;
            float value = leftRed || rightRed ? 0.35f : 1f;
            return value * CrewAlive(
                tank,
                "driver",
                1f,
                0.55f);
        }

        private static float ModuleMultiplier(
            TankState tank,
            string id,
            float yellow,
            float red)
        {
            DamageModuleState module;
            if (!tank.Combat.Modules.TryGetValue(id, out module))
                return 1f;
            return module.Condition == DamageModuleCondition.Red
                ? red
                : module.Condition == DamageModuleCondition.Yellow
                    ? yellow
                    : 1f;
        }

        private static bool IsRed(TankState tank, string id)
        {
            DamageModuleState module;
            return tank.Combat.Modules.TryGetValue(id, out module) &&
                module.Condition == DamageModuleCondition.Red;
        }

        private static float CrewAlive(
            TankState tank,
            string id,
            float alive,
            float disabled)
        {
            bool present;
            return !tank.Combat.Crew.TryGetValue(id, out present) ||
                present
                    ? alive
                    : disabled;
        }
    }
}
