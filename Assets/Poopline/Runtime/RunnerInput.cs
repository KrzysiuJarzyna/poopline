using UnityEngine;
using UnityEngine.InputSystem;

namespace Poopline
{
    public sealed class RunnerInput : System.IDisposable
    {
        private readonly InputAction move = new InputAction("Move", InputActionType.Value);
        private readonly System.Collections.Generic.Dictionary<int, float> touchDirections = new System.Collections.Generic.Dictionary<int, float>();
        private readonly InputAction accept = new InputAction("Accept", InputActionType.Button);
        private readonly InputAction pause = new InputAction("Pause", InputActionType.Button);
        private readonly InputAction retry = new InputAction("Retry", InputActionType.Button);
        public bool AcceptPressed => accept.WasPressedThisFrame();
        public bool PausePressed => pause.WasPressedThisFrame();
        public bool RetryPressed => retry.WasPressedThisFrame();
        public float Horizontal
        {
            get
            {
                float value = move.ReadValue<float>();
                foreach (float direction in touchDirections.Values) value += direction;
                return Mathf.Clamp(value, -1, 1);
            }
        }

        public void HoldDirection(int pointerId, float direction) { touchDirections[pointerId] = Mathf.Sign(direction); }
        public void ReleaseDirection(int pointerId) { touchDirections.Remove(pointerId); }
        public void ClearTouch() { touchDirections.Clear(); }

        public RunnerInput()
        {
            move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
            move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick/x");
            move.AddBinding("<Gamepad>/dpad/x");
            accept.AddBinding("<Keyboard>/space");
            accept.AddBinding("<Keyboard>/enter");
            accept.AddBinding("<Keyboard>/upArrow");
            accept.AddBinding("<Keyboard>/w");
            accept.AddBinding("<Gamepad>/buttonSouth");
            pause.AddBinding("<Keyboard>/escape");
            pause.AddBinding("<Gamepad>/start");
            retry.AddBinding("<Keyboard>/r");
            retry.AddBinding("<Gamepad>/buttonNorth");
            move.Enable(); accept.Enable(); pause.Enable(); retry.Enable();
        }

        public void Dispose()
        {
            move.Dispose(); accept.Dispose(); pause.Dispose(); retry.Dispose();
        }
    }
}
