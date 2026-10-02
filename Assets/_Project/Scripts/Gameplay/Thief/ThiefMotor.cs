using DogHeist.Gameplay.Controls;
using DogHeist.Gameplay.Noise;
using UnityEngine;

namespace DogHeist.Gameplay.Thief
{
    /// <summary>
    /// Di chuyển của trộm (đi, chạy, lom khom) và phát tiếng bước chân theo kiểu di chuyển.
    /// Đọc điều khiển qua ICharacterInput nên không phụ thuộc bàn phím hay tay cầm.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThiefMotor : MonoBehaviour
    {
        private const float GroundedStickVelocity = -2f;
        private const float MovingSpeedThreshold = 0.1f;
        private const float InputDeadZoneSqr = 0.0001f;

        [SerializeField] private ThiefConfig _config;

        [Tooltip("Camera dùng để tính hướng di chuyển. Để trống sẽ dùng Camera.main.")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private ICharacterInput _input;
        private float _verticalVelocity;
        private float _noiseTimer;

        public ThiefConfig Config => _config;

        /// <summary>Tắt khi kết thúc ván hoặc khi có cutscene.</summary>
        public bool InputEnabled { get; set; } = true;

        public bool IsCrouching { get; private set; }

        public bool IsSprinting { get; private set; }

        public bool IsCarrying { get; internal set; }

        public float CurrentSpeed { get; private set; }

        /// <summary>Mức ồn hiện tại từ 0 tới 1, dùng cho thanh tiếng ồn trên HUD.</summary>
        public float NoiseLevel01 { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<ICharacterInput>();

            if (_config == null)
            {
                Debug.LogError($"{name}: chưa gán ThiefConfig cho ThiefMotor.", this);
                enabled = false;
                return;
            }

            if (_input == null)
            {
                Debug.LogError($"{name}: thiếu component cài ICharacterInput (ví dụ LocalPlayerInput).", this);
                enabled = false;
                return;
            }

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var moveInput = InputEnabled ? Vector2.ClampMagnitude(_input.Move, 1f) : Vector2.zero;
            var hasMoveInput = moveInput.sqrMagnitude > InputDeadZoneSqr;

            IsCrouching = InputEnabled && _input.IsCrouching;
            IsSprinting = InputEnabled && _input.IsSprinting && !IsCrouching && hasMoveInput;

            var direction = ToWorldDirection(moveInput);
            var horizontalVelocity = direction * SelectSpeed();
            CurrentSpeed = horizontalVelocity.magnitude;

            UpdateVerticalVelocity(deltaTime);
            _controller.Move((horizontalVelocity + Vector3.up * _verticalVelocity) * deltaTime);

            RotateTowards(direction, deltaTime);
            UpdateNoise(deltaTime);
        }

        private Vector3 ToWorldDirection(Vector2 moveInput)
        {
            var forward = Vector3.forward;
            var right = Vector3.right;

            if (_cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
            }

            return forward * moveInput.y + right * moveInput.x;
        }

        private float SelectSpeed()
        {
            var speed = IsCrouching ? _config.CrouchSpeed
                : IsSprinting ? _config.SprintSpeed
                : _config.WalkSpeed;

            return IsCarrying ? speed * _config.CarrySpeedMultiplier : speed;
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = GroundedStickVelocity;
            }
            else
            {
                _verticalVelocity += _config.Gravity * deltaTime;
            }
        }

        private void RotateTowards(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude < InputDeadZoneSqr)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, _config.RotationSpeed * deltaTime);
        }

        private void UpdateNoise(float deltaTime)
        {
            var radius = CalculateNoiseRadius();
            NoiseLevel01 = _config.MaxNoiseRadius > 0f ? Mathf.Clamp01(radius / _config.MaxNoiseRadius) : 0f;

            if (radius <= 0f)
            {
                // Đứng yên thì im lặng; bước đầu tiên sau đó sẽ phát tiếng ngay.
                _noiseTimer = 0f;
                return;
            }

            _noiseTimer -= deltaTime;
            if (_noiseTimer > 0f)
            {
                return;
            }

            _noiseTimer = _config.NoiseInterval;
            NoiseSystem.Emit(new NoiseEvent(transform.position, radius, NoiseSource.Thief, gameObject));
        }

        private float CalculateNoiseRadius()
        {
            if (CurrentSpeed < MovingSpeedThreshold)
            {
                return 0f;
            }

            var radius = IsCrouching ? _config.CrouchNoiseRadius
                : IsSprinting ? _config.SprintNoiseRadius
                : _config.WalkNoiseRadius;

            return IsCarrying ? radius + _config.CarryNoiseBonus : radius;
        }
    }
}
