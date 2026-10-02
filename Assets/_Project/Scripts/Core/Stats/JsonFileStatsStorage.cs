using System;
using System.IO;
using UnityEngine;

namespace DogHeist.Core.Stats
{
    /// <summary>
    /// Lưu thành tích ra file JSON trong Application.persistentDataPath.
    /// </summary>
    public sealed class JsonFileStatsStorage : IStatsStorage
    {
        private const string DefaultFileName = "player_stats.json";
        private const string LogPrefix = "[Stats]";

        private readonly string _filePath;

        public JsonFileStatsStorage()
            : this(Path.Combine(Application.persistentDataPath, DefaultFileName))
        {
        }

        public JsonFileStatsStorage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Đường dẫn file thành tích không được để trống.", nameof(filePath));
            }

            _filePath = filePath;
        }

        public string FilePath => _filePath;

        public PlayerStats Load()
        {
            if (!File.Exists(_filePath))
            {
                return new PlayerStats();
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonUtility.FromJson<PlayerStats>(json) ?? new PlayerStats();
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"{LogPrefix} File thành tích bị hỏng, tạo thành tích mới. Chi tiết: {ex.Message}");
                BackupCorruptedFile();
                return new PlayerStats();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"{LogPrefix} Không đọc được file '{_filePath}': {ex.Message}");
                return new PlayerStats();
            }
        }

        public void Save(PlayerStats stats)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }

            var tempPath = _filePath + ".tmp";

            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Ghi ra file tạm trước để không làm hỏng bản lưu cũ nếu game bị tắt giữa chừng.
                File.WriteAllText(tempPath, JsonUtility.ToJson(stats, prettyPrint: true));
                File.Copy(tempPath, _filePath, overwrite: true);
                File.Delete(tempPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"{LogPrefix} Không ghi được file '{_filePath}': {ex.Message}");
            }
        }

        private void BackupCorruptedFile()
        {
            var backupPath = _filePath + ".corrupted";

            try
            {
                File.Copy(_filePath, backupPath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogWarning($"{LogPrefix} Không sao lưu được file hỏng: {ex.Message}");
            }
        }
    }
}
