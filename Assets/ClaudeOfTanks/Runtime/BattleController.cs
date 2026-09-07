using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ClaudeOfTanks.Runtime
{
    [DefaultExecutionOrder(-100)]
    public sealed class BattleController : MonoBehaviour
    {
        private readonly Dictionary<string, TankInput> _inputs = new Dictionary<string, TankInput>();
        private readonly Dictionary<string, TankView> _tankViews = new Dictionary<string, TankView>();
        private readonly Dictionary<int, GameObject> _shellViews = new Dictionary<int, GameObject>();
        private readonly List<int> _staleShellIds = new List<int>();
        private BattleSimulation _simulation;
        private TankState _player;
        private Camera _camera;
        private float _accumulator;
        private string _status = "BATTLE";
        private float _statusUntil;
        private GUIStyle _labelStyle;
        private GUIStyle _statusStyle;

        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                InputSystem.AddDevice<Keyboard>();
            }
            if (Mouse.current == null)
            {
                InputSystem.AddDevice<Mouse>();
            }
#endif
            BuildEnvironment();
            StartBattle();
        }

        private void Update()
        {
            if (_simulation == null)
            {
                return;
            }

            float frameTime = Mathf.Min(Time.deltaTime, 0.25f);
            _accumulator += frameTime;
            TankInput playerInput = ReadPlayerInput();
            while (_accumulator >= BattleState.FixedDeltaTime)
            {
                BuildInputs(playerInput);
                _simulation.Step(_inputs, BattleState.FixedDeltaTime);
                ConsumeEvents();
                _accumulator -= BattleState.FixedDeltaTime;
            }

            SyncViews();
            CheckResult();
            if (Input.GetKeyDown(KeyCode.Return) && IsBattleOver())
            {
                StartBattle();
            }
        }

        private void LateUpdate()
        {
            if (_camera == null || _player == null)
            {
                return;
            }

            Vector3 target = _player.Position.ToUnity() + Vector3.up * 1.8f;
            Vector3 forward = new Vector3(Mathf.Sin(_player.Yaw), 0f, Mathf.Cos(_player.Yaw));
            Vector3 desired = target - forward * 12f + Vector3.up * 7f;
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                desired,
                1f - Mathf.Exp(-7f * Time.deltaTime));
            _camera.transform.rotation = Quaternion.LookRotation(
                target + forward * 8f - _camera.transform.position,
                Vector3.up);
        }

        private void StartBattle()
        {
            foreach (TankView view in _tankViews.Values)
            {
                view.Destroy();
            }

            foreach (GameObject shell in _shellViews.Values)
            {
                Destroy(shell);
            }

            _tankViews.Clear();
            _shellViews.Clear();
            _inputs.Clear();

            BattleState state = new BattleState(new FlatHeightField(), 6000u);
            TankSpec medium = TankSpec.Medium();
            TankSpec heavy = TankSpec.Heavy();
            state.Tanks.Add(new TankState("player", Team.Alpha, medium, new Float3(0f, 0f, -34f), 0f));
            state.Tanks.Add(new TankState("alpha-2", Team.Alpha, heavy, new Float3(-13f, 0f, -42f), 0.1f));
            state.Tanks.Add(new TankState("bravo-1", Team.Bravo, heavy, new Float3(0f, 0f, 42f), MathUtil.Pi));
            state.Tanks.Add(new TankState("bravo-2", Team.Bravo, medium, new Float3(15f, 0f, 35f), MathUtil.Pi));
            state.Tanks.Add(new TankState("bravo-3", Team.Bravo, medium, new Float3(-18f, 0f, 31f), MathUtil.Pi));
            _simulation = new BattleSimulation(state);
            _player = state.Tanks[0];
            for (int i = 0; i < state.Tanks.Count; i++)
            {
                TankState tank = state.Tanks[i];
                _tankViews.Add(tank.Id, TankView.Create(tank));
            }

            _accumulator = 0f;
            _status = "BATTLE";
            _statusUntil = Time.unscaledTime + 1.5f;
        }

        private TankInput ReadPlayerInput()
        {
            float throttle = 0f;
            float steer = 0f;
            if (IsKeyPressed(KeyCode.W) || IsKeyPressed(KeyCode.UpArrow)) throttle += 1f;
            if (IsKeyPressed(KeyCode.S) || IsKeyPressed(KeyCode.DownArrow)) throttle -= 1f;
            if (IsKeyPressed(KeyCode.D) || IsKeyPressed(KeyCode.RightArrow)) steer += 1f;
            if (IsKeyPressed(KeyCode.A) || IsKeyPressed(KeyCode.LeftArrow)) steer -= 1f;

            Float3 aimPoint = _player.Position + Float3.Forward(_player.Yaw + _player.TurretYaw) * 100f;
            if (_camera != null)
            {
                Ray ray = _camera.ScreenPointToRay(PointerPosition());
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                float distance;
                if (ground.Raycast(ray, out distance))
                {
                    aimPoint = ray.GetPoint(distance).ToSimulation();
                }
            }

            return new TankInput
            {
                Throttle = throttle,
                Steer = steer,
                Brake = IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift),
                Fire = IsPrimaryButtonPressed() || IsKeyPressed(KeyCode.Space),
                AimPoint = aimPoint
            };
        }

        private static bool IsKeyPressed(KeyCode keyCode)
        {
            bool pressed = Input.GetKey(keyCode);
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return pressed;
            }

            switch (keyCode)
            {
                case KeyCode.W: return pressed || keyboard.wKey.isPressed;
                case KeyCode.A: return pressed || keyboard.aKey.isPressed;
                case KeyCode.S: return pressed || keyboard.sKey.isPressed;
                case KeyCode.D: return pressed || keyboard.dKey.isPressed;
                case KeyCode.UpArrow: return pressed || keyboard.upArrowKey.isPressed;
                case KeyCode.DownArrow: return pressed || keyboard.downArrowKey.isPressed;
                case KeyCode.LeftArrow: return pressed || keyboard.leftArrowKey.isPressed;
                case KeyCode.RightArrow: return pressed || keyboard.rightArrowKey.isPressed;
                case KeyCode.LeftShift: return pressed || keyboard.leftShiftKey.isPressed;
                case KeyCode.RightShift: return pressed || keyboard.rightShiftKey.isPressed;
                case KeyCode.Space: return pressed || keyboard.spaceKey.isPressed;
            }
#endif
            return pressed;
        }

        private static bool IsPrimaryButtonPressed()
        {
            bool pressed = Input.GetMouseButton(0);
#if ENABLE_INPUT_SYSTEM
            pressed |= Mouse.current != null && Mouse.current.leftButton.isPressed;
#endif
            return pressed;
        }

        private static Vector2 PointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
#endif
            return Input.mousePosition;
        }

        private void BuildInputs(TankInput playerInput)
        {
            _inputs.Clear();
            _inputs[_player.Id] = playerInput;
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 1; i < tanks.Count; i++)
            {
                TankState bot = tanks[i];
                if (bot.Destroyed)
                {
                    continue;
                }

                TankState target = FindClosestEnemy(bot, tanks);
                if (target == null)
                {
                    continue;
                }

                Float3 offset = target.Position - bot.Position;
                float distance = offset.Magnitude;
                float desiredYaw = Mathf.Atan2(offset.X, offset.Z);
                float hullDelta = MathUtil.DeltaAngle(bot.Yaw, desiredYaw);
                float gunDelta = MathUtil.DeltaAngle(bot.Yaw + bot.TurretYaw, desiredYaw);
                _inputs[bot.Id] = new TankInput
                {
                    Throttle = distance > 34f ? 1f : distance < 18f ? -0.45f : 0f,
                    Steer = MathUtil.Clamp(hullDelta * 2.2f, -1f, 1f),
                    Brake = distance >= 18f && distance <= 34f,
                    Fire = distance < 105f && Mathf.Abs(gunDelta) < 0.055f,
                    AimPoint = target.Position + new Float3(0f, 1.25f, 0f)
                };
            }
        }

        private static TankState FindClosestEnemy(TankState source, List<TankState> tanks)
        {
            TankState best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < tanks.Count; i++)
            {
                TankState candidate = tanks[i];
                if (candidate.Destroyed || candidate.Team == source.Team)
                {
                    continue;
                }

                float distance = (candidate.Position - source.Position).SqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private void ConsumeEvents()
        {
            List<BattleEvent> events = _simulation.State.Events;
            for (int i = 0; i < events.Count; i++)
            {
                BattleEvent battleEvent = events[i];
                if (battleEvent.Type == BattleEventType.ShellHit)
                {
                    _status = battleEvent.Penetrated
                        ? Mathf.RoundToInt(battleEvent.Value).ToString()
                        : "RICOCHET";
                    _statusUntil = Time.unscaledTime + 0.8f;
                }
                else if (battleEvent.Type == BattleEventType.TankDestroyed)
                {
                    _status = "DESTROYED";
                    _statusUntil = Time.unscaledTime + 1.4f;
                }
            }
        }

        private void SyncViews()
        {
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
            {
                _tankViews[tanks[i].Id].Sync(tanks[i]);
            }

            _staleShellIds.Clear();
            foreach (int id in _shellViews.Keys)
            {
                _staleShellIds.Add(id);
            }

            List<ShellState> shells = _simulation.State.Shells;
            for (int i = 0; i < shells.Count; i++)
            {
                ShellState shell = shells[i];
                GameObject view;
                if (!_shellViews.TryGetValue(shell.Id, out view))
                {
                    view = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    view.name = "Shell-" + shell.Id;
                    view.transform.localScale = Vector3.one * 0.18f;
                    Renderer renderer = view.GetComponent<Renderer>();
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = new Color(1f, 0.72f, 0.14f);
                    _shellViews.Add(shell.Id, view);
                }

                view.transform.position = shell.Position.ToUnity();
                _staleShellIds.Remove(shell.Id);
            }

            for (int i = 0; i < _staleShellIds.Count; i++)
            {
                int id = _staleShellIds[i];
                Destroy(_shellViews[id]);
                _shellViews.Remove(id);
            }
        }

        private void CheckResult()
        {
            bool alphaAlive = false;
            bool bravoAlive = false;
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
            {
                if (tanks[i].Destroyed) continue;
                alphaAlive |= tanks[i].Team == Team.Alpha;
                bravoAlive |= tanks[i].Team == Team.Bravo;
            }

            if (!alphaAlive)
            {
                _status = "DEFEAT";
                _statusUntil = float.PositiveInfinity;
            }
            else if (!bravoAlive)
            {
                _status = "VICTORY";
                _statusUntil = float.PositiveInfinity;
            }
        }

        private bool IsBattleOver()
        {
            return _status == "VICTORY" || _status == "DEFEAT";
        }

        private void BuildEnvironment()
        {
            RenderSettings.ambientLight = new Color(0.34f, 0.37f, 0.40f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.55f, 0.64f, 0.68f);
            RenderSettings.fogDensity = 0.004f;

            GameObject sun = new GameObject("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.94f, 0.80f);
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Battlefield";
            ground.transform.localScale = new Vector3(24f, 1f, 24f);
            Material groundMaterial = new Material(Shader.Find("Standard"));
            groundMaterial.color = new Color(0.28f, 0.34f, 0.22f);
            ground.GetComponent<Renderer>().material = groundMaterial;

            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            _camera.fieldOfView = 58f;
            _camera.nearClipPlane = 0.15f;
            _camera.farClipPlane = 500f;
            _camera.backgroundColor = new Color(0.49f, 0.61f, 0.68f);
            _camera.transform.position = new Vector3(0f, 8f, -48f);
        }

        private void OnGUI()
        {
            EnsureStyles();
            float hpRatio = _player == null ? 0f : _player.Health / _player.Spec.MaxHealth;
            GUI.color = new Color(0.05f, 0.06f, 0.055f, 0.88f);
            GUI.Box(new Rect(20f, Screen.height - 92f, 286f, 66f), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(34f, Screen.height - 82f, 250f, 24f),
                _player == null ? string.Empty : _player.Spec.DisplayName, _labelStyle);
            GUI.Label(new Rect(34f, Screen.height - 56f, 250f, 24f),
                _player == null
                    ? string.Empty
                    : string.Format("HP {0:0}   AP {1:0.0}s", _player.Health, _player.ReloadRemainingS),
                _labelStyle);

            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            GUI.DrawTexture(new Rect(20f, Screen.height - 20f, 286f, 6f), Texture2D.whiteTexture);
            GUI.color = hpRatio > 0.35f ? new Color(0.36f, 0.78f, 0.28f) : new Color(0.88f, 0.18f, 0.12f);
            GUI.DrawTexture(new Rect(20f, Screen.height - 20f, 286f * hpRatio, 6f), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width * 0.5f - 8f, Screen.height * 0.5f - 16f, 20f, 30f), "+", _statusStyle);
            if (Time.unscaledTime < _statusUntil)
            {
                GUI.Label(new Rect(0f, 30f, Screen.width, 48f), _status, _statusStyle);
            }

            if (IsBattleOver() &&
                GUI.Button(new Rect(Screen.width * 0.5f - 70f, Screen.height * 0.5f + 58f, 140f, 38f), "RESTART"))
            {
                StartBattle();
            }

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }
    }
}
