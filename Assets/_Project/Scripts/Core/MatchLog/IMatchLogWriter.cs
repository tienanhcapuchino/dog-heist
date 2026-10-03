namespace DogHeist.Core.MatchLog
{
    /// <summary>Nơi ghi nhật ký ván chơi; tách interface để test không cần ghi file thật.</summary>
    public interface IMatchLogWriter
    {
        void Append(MatchLogEntry entry);
    }
}
