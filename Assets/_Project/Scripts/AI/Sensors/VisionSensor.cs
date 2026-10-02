using DogHeist.Gameplay.Thief;
using UnityEngine;

namespace DogHeist.AI.Sensors
{
    /// <summary>
    /// "Đôi mắt" của AI: tầm nhìn hình nón, bị che bởi vật cản,
    /// tầm xa thực tế = tầm nhìn tối đa x độ lộ diện của trộm.
    /// </summary>
    public sealed class VisionSensor : MonoBehaviour
    {
        private const float DefaultEyeHeight = 1.6f;

        [SerializeField, Min(0f)] private float _range = 12f;
        [SerializeField, Range(1f, 360f)] private float _fieldOfView = 110f;

        [Tooltip("Vị trí mắt. Để trống sẽ dùng vị trí nhân vật cộng 1.6m.")]
        [SerializeField] private Transform _eye;

        [Tooltip("Các layer có thể che tầm nhìn (tường, hàng rào, nhân vật).")]
        [SerializeField] private LayerMask _obstacleLayers = ~0;

        public float Range => _range;

        private Vector3 EyePosition => _eye != null ? _eye.position : transform.position + Vector3.up * DefaultEyeHeight;

        private Vector3 Forward => _eye != null ? _eye.forward : transform.forward;

        public bool CanSee(ThiefVisibility target)
        {
            if (target == null || !target.isActiveAndEnabled)
            {
                return false;
            }

            var eyePosition = EyePosition;
            var toTarget = target.SightPoint - eyePosition;
            var effectiveRange = _range * target.Visibility01;

            if (toTarget.sqrMagnitude > effectiveRange * effectiveRange)
            {
                return false;
            }

            if (Vector3.Angle(Forward, toTarget) > _fieldOfView * 0.5f)
            {
                return false;
            }

            var distance = toTarget.magnitude;
            if (distance < Mathf.Epsilon)
            {
                return true;
            }

            if (Physics.Raycast(eyePosition, toTarget / distance, out var hit, distance, _obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.transform.IsChildOf(target.transform);
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            var eyePosition = EyePosition;
            var halfAngle = _fieldOfView * 0.5f;
            var left = Quaternion.AngleAxis(-halfAngle, Vector3.up) * Forward;
            var right = Quaternion.AngleAxis(halfAngle, Vector3.up) * Forward;

            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            Gizmos.DrawLine(eyePosition, eyePosition + left * _range);
            Gizmos.DrawLine(eyePosition, eyePosition + right * _range);
            Gizmos.DrawWireSphere(eyePosition, _range);
        }
    }
}
