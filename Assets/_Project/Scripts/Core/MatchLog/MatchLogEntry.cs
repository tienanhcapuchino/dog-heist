using System;
using UnityEngine;

namespace DogHeist.Core.MatchLog
{
    /// <summary>
    /// Nhật ký một ván chơi, ghi thành một dòng JSON để phân tích khi cân bằng thông số.
    /// Theo mẫu PlayerStats: field private có tiền tố _, nên khóa JSON cũng có dấu _ đầu.
    /// Mốc không xảy ra có giá trị -1 (MatchLogBuilder.NotReached).
    /// </summary>
    [Serializable]
    public sealed class MatchLogEntry
    {
        [SerializeField] private string _endedAtUtc;
        [SerializeField] private string _outcome;
        [SerializeField] private float _durationSeconds;
        [SerializeField] private string _configHash;

        [SerializeField] private float _firstBarkSeconds;
        [SerializeField] private float _firstOwnerWakeSeconds;
        [SerializeField] private float _firstSpottedSeconds;
        [SerializeField] private float _firstLureThrownSeconds;
        [SerializeField] private float _firstDogLuredSeconds;
        [SerializeField] private float _firstPickupSeconds;

        [SerializeField] private int _barkCount;
        [SerializeField] private int _ownerWakeCount;
        [SerializeField] private int _spottedCount;
        [SerializeField] private int _luresThrown;
        [SerializeField] private int _dogDropCount;

        [SerializeField] private float _secondsInLight;
        [SerializeField] private float _secondsHidden;
        [SerializeField] private float _secondsCrouching;
        [SerializeField] private float _secondsSprinting;

        internal MatchLogEntry()
        {
        }

        /// <summary>Thời điểm kết thúc ván, chuỗi ISO 8601 theo giờ UTC.</summary>
        public string EndedAtUtc { get => _endedAtUtc; internal set => _endedAtUtc = value; }

        /// <summary>Tên của MatchOutcome (ThiefEscaped hoặc ThiefCaught).</summary>
        public string Outcome { get => _outcome; internal set => _outcome = value; }

        public float DurationSeconds { get => _durationSeconds; internal set => _durationSeconds = value; }

        /// <summary>Mã 8 ký tự của bộ thông số, dùng để gom các ván cùng một vòng cân bằng.</summary>
        public string ConfigHash { get => _configHash; internal set => _configHash = value; }

        public float FirstBarkSeconds { get => _firstBarkSeconds; internal set => _firstBarkSeconds = value; }

        public float FirstOwnerWakeSeconds { get => _firstOwnerWakeSeconds; internal set => _firstOwnerWakeSeconds = value; }

        public float FirstSpottedSeconds { get => _firstSpottedSeconds; internal set => _firstSpottedSeconds = value; }

        public float FirstLureThrownSeconds { get => _firstLureThrownSeconds; internal set => _firstLureThrownSeconds = value; }

        public float FirstDogLuredSeconds { get => _firstDogLuredSeconds; internal set => _firstDogLuredSeconds = value; }

        public float FirstPickupSeconds { get => _firstPickupSeconds; internal set => _firstPickupSeconds = value; }

        public int BarkCount { get => _barkCount; internal set => _barkCount = value; }

        public int OwnerWakeCount { get => _ownerWakeCount; internal set => _ownerWakeCount = value; }

        public int SpottedCount { get => _spottedCount; internal set => _spottedCount = value; }

        public int LuresThrown { get => _luresThrown; internal set => _luresThrown = value; }

        public int DogDropCount { get => _dogDropCount; internal set => _dogDropCount = value; }

        public float SecondsInLight { get => _secondsInLight; internal set => _secondsInLight = value; }

        public float SecondsHidden { get => _secondsHidden; internal set => _secondsHidden = value; }

        public float SecondsCrouching { get => _secondsCrouching; internal set => _secondsCrouching = value; }

        public float SecondsSprinting { get => _secondsSprinting; internal set => _secondsSprinting = value; }
    }
}
