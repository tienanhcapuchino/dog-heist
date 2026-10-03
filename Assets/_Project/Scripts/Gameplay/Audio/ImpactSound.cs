using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>Gắn trên prefab đồ ăn: phát tiếng "bịch" ở lần va chạm đầu tiên (không kêu mỗi lần nảy).</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ImpactSound : MonoBehaviour
    {
        [SerializeField] private SoundLibrary _library;

        private bool _hasPlayed;

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasPlayed || _library == null)
            {
                return;
            }

            _hasPlayed = true;
            AudioCuePlayer.Play(_library.LureLand, GetComponent<AudioSource>());
        }
    }
}
