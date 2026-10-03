using System;
using System.IO;
using UnityEngine;

namespace DogHeist.Core.MatchLog
{
    /// <summary>Ghi mỗi ván một dòng JSON vào match_log.jsonl (định dạng JSON Lines).</summary>
    public sealed class JsonLinesMatchLogWriter : IMatchLogWriter
    {
        private const string FileName = "match_log.jsonl";

        public JsonLinesMatchLogWriter()
            : this(Path.Combine(Application.persistentDataPath, FileName))
        {
        }

        public JsonLinesMatchLogWriter(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Đường dẫn file nhật ký không được rỗng.", nameof(filePath));
            }

            FilePath = filePath;
        }

        public string FilePath { get; }

        public void Append(MatchLogEntry entry)
        {
            try
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(FilePath, JsonUtility.ToJson(entry) + "\n");
            }
            catch (IOException exception)
            {
                Debug.LogError($"[MatchLog] Không ghi được nhật ký ván chơi vào '{FilePath}': {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                Debug.LogError($"[MatchLog] Không có quyền ghi nhật ký ván chơi vào '{FilePath}': {exception.Message}");
            }
        }
    }
}
