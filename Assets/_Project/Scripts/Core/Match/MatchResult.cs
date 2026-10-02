namespace DogHeist.Core.Match
{
    /// <summary>
    /// Dữ liệu tổng kết một ván, dùng để ghi thành tích và hiển thị màn hình kết quả.
    /// </summary>
    public readonly struct MatchResult
    {
        public MatchResult(PlayerRole localRole, MatchOutcome outcome, float durationSeconds, bool wasThiefSpotted)
        {
            LocalRole = localRole;
            Outcome = outcome;
            DurationSeconds = durationSeconds;
            WasThiefSpotted = wasThiefSpotted;
        }

        public PlayerRole LocalRole { get; }

        public MatchOutcome Outcome { get; }

        public float DurationSeconds { get; }

        /// <summary>Trộm có bị chủ nhà nhìn thấy lần nào trong ván không.</summary>
        public bool WasThiefSpotted { get; }

        public bool LocalPlayerWon => LocalRole switch
        {
            PlayerRole.Thief => Outcome == MatchOutcome.ThiefEscaped,
            PlayerRole.Owner => Outcome == MatchOutcome.ThiefCaught,
            _ => false
        };
    }
}
