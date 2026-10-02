using System;
using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using UnityEngine;

namespace DogHeist.Gameplay.Match
{
    /// <summary>
    /// Kênh sự kiện của trận đấu, giúp AI và UI không phải tham chiếu trực tiếp lẫn nhau.
    /// Bản chơi đơn: mọi thứ chạy cục bộ. Bản multiplayer: chỉ server được gọi các hàm Raise.
    /// </summary>
    public static class MatchEvents
    {
        public static event Action ThiefSpotted;

        public static event Action ThiefCaught;

        public static event Action ThiefEscaped;

        public static event Action<MatchResult, PlayerStats> MatchEnded;

        public static void RaiseThiefSpotted() => ThiefSpotted?.Invoke();

        public static void RaiseThiefCaught() => ThiefCaught?.Invoke();

        public static void RaiseThiefEscaped() => ThiefEscaped?.Invoke();

        public static void RaiseMatchEnded(MatchResult result, PlayerStats stats) => MatchEnded?.Invoke(result, stats);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ThiefSpotted = null;
            ThiefCaught = null;
            ThiefEscaped = null;
            MatchEnded = null;
        }
    }
}
