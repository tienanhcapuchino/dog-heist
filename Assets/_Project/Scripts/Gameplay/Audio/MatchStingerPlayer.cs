using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using DogHeist.Gameplay.Match;
using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>Phát đoạn nhạc ngắn khi ván kết thúc: thắng (trốn thoát) hoặc thua (bị bắt).</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class MatchStingerPlayer : MonoBehaviour
    {
        [SerializeField] private SoundLibrary _library;

        private void OnEnable() => MatchEvents.MatchEnded += HandleMatchEnded;

        private void OnDisable() => MatchEvents.MatchEnded -= HandleMatchEnded;

        private void HandleMatchEnded(MatchResult result, PlayerStats stats)
        {
            if (_library == null)
            {
                return;
            }

            var cue = result.LocalPlayerWon ? _library.StingerWin : _library.StingerLose;
            AudioCuePlayer.Play(cue, GetComponent<AudioSource>());
        }
    }
}
