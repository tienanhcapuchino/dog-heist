using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>Một lần phát đã chọn sẵn: clip, âm lượng và pitch. Clip null nghĩa là không phát gì.</summary>
    public readonly struct CuePlayback
    {
        public CuePlayback(AudioClip clip, float volume, float pitch)
        {
            Clip = clip;
            Volume = volume;
            Pitch = pitch;
        }

        public AudioClip Clip { get; }

        public float Volume { get; }

        public float Pitch { get; }
    }
}
