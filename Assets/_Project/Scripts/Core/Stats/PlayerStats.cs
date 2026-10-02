using System;
using UnityEngine;

namespace DogHeist.Core.Stats
{
    /// <summary>
    /// Thành tích lâu dài của người chơi, được lưu ra file JSON.
    /// Khi đổi cấu trúc dữ liệu, tăng CurrentSaveVersion và viết code chuyển đổi bản lưu cũ.
    /// </summary>
    [Serializable]
    public sealed class PlayerStats
    {
        public const int CurrentSaveVersion = 1;

        [SerializeField] private int _saveVersion = CurrentSaveVersion;
        [SerializeField] private int _successfulHeists;
        [SerializeField] private int _cleanHeists;
        [SerializeField] private int _timesCaught;
        [SerializeField] private int _currentHeistStreak;
        [SerializeField] private int _bestHeistStreak;
        [SerializeField] private int _thievesCaught;

        public int SaveVersion => _saveVersion;

        /// <summary>Số lần trộm thành công mà không bị bắt.</summary>
        public int SuccessfulHeists
        {
            get => _successfulHeists;
            internal set => _successfulHeists = value;
        }

        /// <summary>Số lần trộm thành công mà chủ nhà không hề nhìn thấy.</summary>
        public int CleanHeists
        {
            get => _cleanHeists;
            internal set => _cleanHeists = value;
        }

        public int TimesCaught
        {
            get => _timesCaught;
            internal set => _timesCaught = value;
        }

        public int CurrentHeistStreak
        {
            get => _currentHeistStreak;
            internal set => _currentHeistStreak = value;
        }

        public int BestHeistStreak
        {
            get => _bestHeistStreak;
            internal set => _bestHeistStreak = value;
        }

        /// <summary>Số lần bắt được trộm khi chơi vai chủ nhà (dùng từ bản sau).</summary>
        public int ThievesCaught
        {
            get => _thievesCaught;
            internal set => _thievesCaught = value;
        }
    }
}
