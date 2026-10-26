using UnityEngine;
using UnityEngine.InputSystem;
namespace Poopline
{
    public sealed class RunnerInput : System.IDisposable
    {
        private readonly InputAction move = new InputAction("Move", InputActionType.Value);
        private readonly InputAction pause = new InputAction("Pause", InputActionType.Button);
        private readonly InputAction retry = new InputAction("Retry", InputActionType.Button);
        public Vector2 Move => Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1);
        public bool PausePressed => pause.WasPressedThisFrame();
        public bool RetryPressed => retry.WasPressedThisFrame();
        public RunnerInput()
        {
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            pause.AddBinding("<Keyboard>/escape"); pause.AddBinding("<Gamepad>/start");
            retry.AddBinding("<Keyboard>/r");
            move.Enable(); pause.Enable(); retry.Enable();
        }
        public void Dispose() { move.Dispose(); pause.Dispose(); retry.Dispose(); }
    }
}
