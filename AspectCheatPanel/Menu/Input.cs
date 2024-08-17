using UnityEngine;
using UnityEngine.InputSystem;

namespace Aspect.MenuLib
{
    /// <summary>
    /// This class handles all controller input.
    /// </summary>
    public class Input
    {
        public static Input instance = new Input();

        public bool CheckButton(ButtonType type, bool leftHand = true)
        {
            if (leftHand)
            {
                switch (type)
                {
                    case ButtonType.trigger: return ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f || Mouse.current.leftButton.isPressed;
                    case ButtonType.grip: return ControllerInputPoller.instance.leftControllerGripFloat > 0.5f || Mouse.current.rightButton.isPressed;
                    case ButtonType.secondary: return ControllerInputPoller.instance.leftControllerSecondaryButton || Keyboard.current.qKey.isPressed;
                    case ButtonType.primary: return ControllerInputPoller.instance.leftControllerPrimaryButton || Keyboard.current.eKey.isPressed;

                    default:
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case ButtonType.trigger: return ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || Mouse.current.leftButton.isPressed;
                    case ButtonType.grip: return ControllerInputPoller.instance.rightControllerGripFloat > 0.5f || Mouse.current.rightButton.isPressed;
                    case ButtonType.secondary: return ControllerInputPoller.instance.rightControllerSecondaryButton || Keyboard.current.qKey.isPressed;
                    case ButtonType.primary: return ControllerInputPoller.instance.rightControllerPrimaryButton || Keyboard.current.eKey.isPressed;

                    default:
                        break;
                }
            }
            return false;
        }

        public Vector2 GetJoystickVector()
        {
            return ControllerInputPoller.instance.rightControllerPrimary2DAxis;
        }

        public enum ButtonType
        {
            grip,
            trigger,
            primary,
            secondary
        }
    }
}
