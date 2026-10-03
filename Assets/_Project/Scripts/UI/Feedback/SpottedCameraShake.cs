using DogHeist.Gameplay.Match;
using Unity.Cinemachine;
using UnityEngine;

namespace DogHeist.UI.Feedback
{
    /// <summary>
    /// Rung camera ngắn khi chủ nhà phát hiện trộm (không rung khi chó sủa).
    /// Camera cần có CinemachineImpulseListener để nhận cú rung.
    /// </summary>
    public sealed class SpottedCameraShake : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource _impulse;

        [Tooltip("Độ mạnh cú rung.")]
        [SerializeField, Min(0f)] private float _force = 0.6f;

        private void OnEnable() => MatchEvents.ThiefSpotted += HandleThiefSpotted;

        private void OnDisable() => MatchEvents.ThiefSpotted -= HandleThiefSpotted;

        private void HandleThiefSpotted()
        {
            if (_impulse != null)
            {
                _impulse.GenerateImpulseWithForce(_force);
            }
        }
    }
}
