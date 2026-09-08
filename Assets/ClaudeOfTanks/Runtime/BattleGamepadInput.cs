using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ClaudeOfTanks.Runtime
{
    internal struct BattleGamepadFrame
    {
        public float Steer;
        public float Throttle;
        public bool Fire;
        public bool Brake;
        public bool SniperPressed;
        public bool RepairPressed;
        public bool FirstAidPressed;
        public bool ExtinguisherPressed;
        public bool HydropneumaticAimPressed;
    }

    internal static class BattleGamepadInput
    {
        public static BattleGamepadFrame Read()
        {
            BattleGamepadFrame frame = default;
#if ENABLE_INPUT_SYSTEM
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return frame;
            }

            Vector2 stick = gamepad.leftStick.ReadValue();
            frame.Steer = stick.x;
            frame.Throttle = stick.y;
            frame.Fire = gamepad.rightTrigger.isPressed;
            frame.Brake = gamepad.buttonSouth.isPressed;
            frame.SniperPressed =
                gamepad.leftTrigger.wasPressedThisFrame;
            frame.RepairPressed =
                gamepad.buttonWest.wasPressedThisFrame;
            frame.FirstAidPressed =
                gamepad.buttonNorth.wasPressedThisFrame;
            frame.ExtinguisherPressed =
                gamepad.buttonEast.wasPressedThisFrame;
            frame.HydropneumaticAimPressed =
                gamepad.rightStickButton.wasPressedThisFrame;
#endif
            return frame;
        }
    }
}
