using UnityEngine;

namespace DogHeist.Gameplay.Thief
{
    /// <summary>
    /// Tính độ lộ diện của trộm dựa trên ánh sáng, chỗ nấp và tư thế.
    /// VisionSensor của chủ nhà nhân tầm nhìn với giá trị này: càng tối, càng phải lại gần mới thấy.
    /// </summary>
    [RequireComponent(typeof(ThiefMotor))]
    public sealed class ThiefVisibility : MonoBehaviour
    {
        private const float StandingSightHeight = 1.2f;
        private const float CrouchingSightHeight = 0.6f;

        [SerializeField] private ThiefMotor _motor;

        private int _lightZoneCount;
        private int _hidingSpotCount;

        public bool IsInLight => _lightZoneCount > 0;

        public bool IsHidden => _hidingSpotCount > 0 && !IsInLight && _motor.IsCrouching;

        /// <summary>Điểm mà AI nhắm tới khi kiểm tra tầm nhìn.</summary>
        public Vector3 SightPoint =>
            transform.position + Vector3.up * (_motor.IsCrouching ? CrouchingSightHeight : StandingSightHeight);

        public float Visibility01
        {
            get
            {
                var config = _motor.Config;
                if (config == null)
                {
                    return 1f;
                }

                if (IsHidden)
                {
                    return config.HiddenVisibility;
                }

                var visibility = IsInLight ? config.LitVisibility : config.DarkVisibility;
                if (_motor.IsCrouching)
                {
                    visibility *= config.CrouchVisibilityMultiplier;
                }

                return Mathf.Clamp01(visibility);
            }
        }

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponent<ThiefMotor>();
            }
        }

        internal void EnterLight() => _lightZoneCount++;

        internal void ExitLight() => _lightZoneCount = Mathf.Max(0, _lightZoneCount - 1);

        internal void EnterHidingSpot() => _hidingSpotCount++;

        internal void ExitHidingSpot() => _hidingSpotCount = Mathf.Max(0, _hidingSpotCount - 1);
    }
}
