using System;
using DogHeist.Core.Match;

namespace DogHeist.Core.Stats
{
    /// <summary>
    /// Luật tính thành tích. Tách khỏi MonoBehaviour để test được và dùng lại khi lên multiplayer.
    /// </summary>
    public sealed class StatsService
    {
        private readonly IStatsStorage _storage;

        public StatsService(IStatsStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Current = _storage.Load() ?? new PlayerStats();
        }

        public event Action<PlayerStats> StatsChanged;

        public PlayerStats Current { get; }

        public void Record(MatchResult result)
        {
            if (result.Outcome == MatchOutcome.None)
            {
                return;
            }

            switch (result.LocalRole)
            {
                case PlayerRole.Thief:
                    RecordThiefMatch(result);
                    break;
                case PlayerRole.Owner:
                    RecordOwnerMatch(result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result.LocalRole, "Vai người chơi không hợp lệ.");
            }

            _storage.Save(Current);
            StatsChanged?.Invoke(Current);
        }

        private void RecordThiefMatch(MatchResult result)
        {
            if (result.Outcome == MatchOutcome.ThiefCaught)
            {
                Current.TimesCaught++;
                Current.CurrentHeistStreak = 0;
                return;
            }

            Current.SuccessfulHeists++;
            Current.CurrentHeistStreak++;

            if (!result.WasThiefSpotted)
            {
                Current.CleanHeists++;
            }

            if (Current.CurrentHeistStreak > Current.BestHeistStreak)
            {
                Current.BestHeistStreak = Current.CurrentHeistStreak;
            }
        }

        private void RecordOwnerMatch(MatchResult result)
        {
            if (result.Outcome == MatchOutcome.ThiefCaught)
            {
                Current.ThievesCaught++;
            }
        }
    }
}
