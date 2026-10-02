using UnityEngine;

namespace DogHeist.Gameplay.Noise
{
    /// <summary>
    /// Một tiếng động trong thế giới game: phát ra tại một điểm, nghe được trong bán kính nhất định.
    /// </summary>
    public readonly struct NoiseEvent
    {
        public NoiseEvent(Vector3 position, float radius, NoiseSource source, GameObject emitter)
        {
            Position = position;
            Radius = Mathf.Max(0f, radius);
            Source = source;
            Emitter = emitter;
        }

        public Vector3 Position { get; }

        public float Radius { get; }

        public NoiseSource Source { get; }

        /// <summary>Đối tượng phát ra tiếng động, dùng để bỏ qua tiếng của chính mình.</summary>
        public GameObject Emitter { get; }

        /// <param name="sensitivity">Độ thính tai của người nghe: 1 là bình thường, 0.5 là đang ngủ say.</param>
        public bool IsAudibleFrom(Vector3 listenerPosition, float sensitivity = 1f)
        {
            var effectiveRadius = Radius * Mathf.Max(0f, sensitivity);
            if (effectiveRadius <= 0f)
            {
                return false;
            }

            return (listenerPosition - Position).sqrMagnitude <= effectiveRadius * effectiveRadius;
        }
    }
}
