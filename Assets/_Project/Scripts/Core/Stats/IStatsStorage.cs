namespace DogHeist.Core.Stats
{
    /// <summary>
    /// Nơi lưu thành tích. Bản đầu dùng file JSON trên máy;
    /// sau này có thể thêm bản cài đặt dùng Steam Stats hoặc server.
    /// </summary>
    public interface IStatsStorage
    {
        PlayerStats Load();

        void Save(PlayerStats stats);
    }
}
