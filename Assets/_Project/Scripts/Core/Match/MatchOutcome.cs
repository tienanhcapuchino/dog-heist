namespace DogHeist.Core.Match
{
    /// <summary>
    /// Kết quả của một ván, mô tả theo góc nhìn sự việc (không theo phe thắng).
    /// </summary>
    public enum MatchOutcome
    {
        None = 0,
        ThiefEscaped = 1,
        ThiefCaught = 2
    }
}
