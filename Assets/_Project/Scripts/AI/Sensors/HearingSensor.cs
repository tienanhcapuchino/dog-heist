using System;
using DogHeist.Gameplay.Noise;
using UnityEngine;

namespace DogHeist.AI.Sensors
{
    /// <summary>
    /// "Đôi tai" của AI: lọc các tiếng động nghe được và báo qua sự kiện NoiseHeard.
    /// </summary>
    public sealed class HearingSensor : MonoBehaviour
    {
        [Tooltip("1 = nghe bình thường. Nhỏ hơn 1 = nghe kém (ví dụ đang ngủ).")]
        [SerializeField, Min(0f)] private float _sensitivity = 1f;

        public event Action<NoiseEvent> NoiseHeard;

        public float Sensitivity
        {
            get => _sensitivity;
            set => _sensitivity = Mathf.Max(0f, value);
        }

        private void OnEnable() => NoiseSystem.NoiseEmitted += HandleNoiseEmitted;

        private void OnDisable() => NoiseSystem.NoiseEmitted -= HandleNoiseEmitted;

        private void HandleNoiseEmitted(NoiseEvent noise)
        {
            if (IsOwnOrCarrierNoise(noise) || !noise.IsAudibleFrom(transform.position, _sensitivity))
            {
                return;
            }

            NoiseHeard?.Invoke(noise);
        }

        // Bỏ qua tiếng của chính mình, và tiếng của người đang bế mình (chó bị bế không phản ứng với trộm).
        private bool IsOwnOrCarrierNoise(NoiseEvent noise) =>
            noise.Emitter != null && transform.IsChildOf(noise.Emitter.transform);
    }
}
