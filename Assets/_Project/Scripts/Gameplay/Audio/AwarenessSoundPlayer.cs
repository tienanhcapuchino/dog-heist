using DogHeist.Gameplay.Awareness;
using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>
    /// Gắn trên chó hoặc chủ nhà: phát "Hửm?", "Trộm!" hay tiếng chó vui khi mức cảnh giác đổi.
    /// Chọn tiếng theo cặp (mức cũ, mức mới) bằng SoundRules.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class AwarenessSoundPlayer : MonoBehaviour
    {
        [Tooltip("DogAI hoặc OwnerAI (component cài IAwarenessSource).")]
        [SerializeField] private MonoBehaviour _source;
        [SerializeField] private SoundLibrary _library;
        [SerializeField] private AwarenessVoice _voice;

        private AudioSource _audioSource;
        private IAwarenessSource _bound;
        private AwarenessLevel _previous;

        private void Awake() => _audioSource = GetComponent<AudioSource>();

        private void OnEnable()
        {
            _bound = AwarenessSourceResolver.Resolve(_source, this);
            if (_bound == null)
            {
                enabled = false;
                return;
            }

            _previous = _bound.Awareness;
            _bound.AwarenessChanged += HandleAwarenessChanged;
        }

        private void OnDisable()
        {
            if (_bound != null)
            {
                _bound.AwarenessChanged -= HandleAwarenessChanged;
                _bound = null;
            }
        }

        private void HandleAwarenessChanged(AwarenessLevel next)
        {
            var cue = SoundRules.ForAwarenessChange(_library, _voice, _previous, next);
            _previous = next;
            AudioCuePlayer.Play(cue, _audioSource);
        }
    }
}
