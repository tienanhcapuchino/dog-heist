using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>Phát một AudioCue qua một AudioSource có sẵn.</summary>
    public static class AudioCuePlayer
    {
        public static void Play(AudioCue cue, AudioSource source)
        {
            if (cue == null || source == null)
            {
                return;
            }

            var playback = cue.Prepare();
            if (playback.Clip == null)
            {
                return;
            }

            source.spatialBlend = cue.SpatialBlend;
            source.maxDistance = cue.MaxDistance;
            source.pitch = playback.Pitch;
            source.PlayOneShot(playback.Clip, playback.Volume);
        }
    }
}
