using UnityEngine;
using UnityEngine.InputSystem;
namespace Poopline
{
    public sealed class RunnerInput : System.IDisposable
    {
        private readonly InputAction move = new InputAction("Move", InputActionType.Value);
        private readonly InputAction pause = new InputAction("Pause", InputActionType.Button);
        private readonly InputAction map = new InputAction("Map", InputActionType.Button);
        private readonly InputAction interact = new InputAction("Interact", InputActionType.Button);
        private readonly InputAction confirm = new InputAction("Confirm", InputActionType.Button);
        private readonly InputAction retry = new InputAction("Retry", InputActionType.Button);
        public Vector2 Move => Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1);
        public bool PausePressed => pause.WasPressedThisFrame();
        public bool MapPressed => map.WasPressedThisFrame();
        public bool InteractHeld => interact.IsPressed();
        public bool ConfirmPressed => confirm.WasPressedThisFrame();
        public bool RetryPressed => retry.WasPressedThisFrame();
        public RunnerInput()
        {
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            pause.AddBinding("<Keyboard>/escape"); pause.AddBinding("<Gamepad>/start");
            retry.AddBinding("<Keyboard>/r");
            map.AddBinding("<Keyboard>/m"); map.AddBinding("<Keyboard>/e"); map.AddBinding("<Gamepad>/select");
            interact.AddBinding("<Keyboard>/f"); interact.AddBinding("<Gamepad>/buttonSouth");
            confirm.AddBinding("<Keyboard>/enter");
            interact.Enable(); confirm.Enable();
            move.Enable(); pause.Enable(); retry.Enable(); map.Enable();
        }
        public void Dispose() { move.Dispose(); pause.Dispose(); retry.Dispose(); map.Dispose(); interact.Dispose(); confirm.Dispose(); }
    }
}
