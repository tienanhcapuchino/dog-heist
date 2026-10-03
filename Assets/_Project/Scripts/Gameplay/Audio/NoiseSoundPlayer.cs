using DogHeist.Gameplay.Noise;
using DogHeist.Gameplay.Thief;
using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    public enum NoiseSoundKind
    {
        /// <summary>Bước chân của trộm, chọn theo lom khom / đi / chạy.</summary>
        Footsteps = 0,

        /// <summary>Tiếng chó sủa.</summary>
        Bark = 1,
    }

    /// <summary>
    /// Phát tiếng mỗi khi chính object này phát tiếng ồn qua NoiseSystem.
    /// Tiếng ồn (luật chơi) và âm thanh (thứ người chơi nghe) đi cùng nhịp nhưng tách riêng: không sửa ThiefMotor hay DogAI.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class NoiseSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SoundLibrary _library;
        [SerializeField] private NoiseSoundKind _kind;

        private AudioSource _audioSource;
        private ThiefMotor _motor;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _motor = GetComponent<ThiefMotor>();

            if (_kind == NoiseSoundKind.Footsteps && _motor == null)
            {
                Debug.LogError("[Audio] NoiseSoundPlayer kiểu Footsteps cần ThiefMotor trên cùng object.", this);
            }
        }

        private void OnEnable() => NoiseSystem.NoiseEmitted += HandleNoiseEmitted;

        private void OnDisable() => NoiseSystem.NoiseEmitted -= HandleNoiseEmitted;

        private void HandleNoiseEmitted(NoiseEvent noise)
        {
            if (noise.Emitter != gameObject || _library == null)
            {
                return;
            }

            AudioCuePlayer.Play(PickCue(), _audioSource);
        }

        private AudioCue PickCue()
        {
            if (_kind == NoiseSoundKind.Bark)
            {
                return _library.DogBark;
            }

            return _motor != null ? SoundRules.Footstep(_library, _motor.IsCrouching, _motor.IsSprinting) : null;
        }
    }
}
