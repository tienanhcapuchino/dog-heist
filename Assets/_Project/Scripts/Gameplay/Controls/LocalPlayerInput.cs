using UnityEngine;
using UnityEngine.InputSystem;

namespace DogHeist.Gameplay.Controls
{
    /// <summary>
    /// Input của người chơi tại máy, hỗ trợ cả bàn phím chuột và tay cầm.
    /// Các action được tạo bằng code cho gọn; khi cần cho phép đổi phím,
    /// chuyển sang file .inputactions và dùng InputActionReference.
    /// </summary>
    public sealed class LocalPlayerInput : MonoBehaviour, ICharacterInput
    {
        private InputAction _move;
        private InputAction _sprint;
        private InputAction _crouch;
        private InputAction _interact;
        private InputAction _throwLure;
        private InputAction[] _allActions;

        public Vector2 Move => _move.ReadValue<Vector2>();

        public bool IsSprinting => _sprint.IsPressed();

        public bool IsCrouching => _crouch.IsPressed();

        public bool InteractPressedThisFrame => _interact.WasPressedThisFrame();

        public bool ThrowLurePressedThisFrame => _throwLure.WasPressedThisFrame();

        private void Awake()
        {
            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _move.AddBinding("<Gamepad>/leftStick");

            _sprint = CreateButton("Sprint", "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            _crouch = CreateButton("Crouch", "<Keyboard>/leftCtrl", "<Gamepad>/buttonEast");
            _interact = CreateButton("Interact", "<Keyboard>/e", "<Gamepad>/buttonSouth");
            _throwLure = CreateButton("ThrowLure", "<Keyboard>/q", "<Gamepad>/rightTrigger");
            _throwLure.AddBinding("<Mouse>/leftButton");

            _allActions = new[] { _move, _sprint, _crouch, _interact, _throwLure };
        }

        private void OnEnable()
        {
            foreach (var action in _allActions)
            {
                action.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (var action in _allActions)
            {
                action.Disable();
            }
        }

        private void OnDestroy()
        {
            foreach (var action in _allActions)
            {
                action.Dispose();
            }
        }

        private static InputAction CreateButton(string name, string keyboardBinding, string gamepadBinding)
        {
            var action = new InputAction(name, InputActionType.Button);
            action.AddBinding(keyboardBinding);
            action.AddBinding(gamepadBinding);
            return action;
        }
    }
}
