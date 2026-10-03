using System;
using System.Globalization;
using DogHeist.Core.Match;

namespace DogHeist.Core.MatchLog
{
    /// <summary>
    /// Gom số liệu trong lúc chơi rồi tạo MatchLogEntry khi ván kết thúc.
    /// Lớp thuần, không cần scene nên test được bằng EditMode test.
    /// </summary>
    public sealed class MatchLogBuilder
    {
        /// <summary>Giá trị của mốc chưa xảy ra.</summary>
        public const float NotReached = -1f;

        private readonly float[] _firsts = CreateFirsts();
        private readonly int[] _counts = new int[Enum.GetValues(typeof(MatchCounter)).Length];
        private readonly float[] _times = new float[Enum.GetValues(typeof(ThiefStateTime)).Length];

        /// <summary>Ghi thời điểm của mốc; chỉ lần đầu được giữ, các lần sau bỏ qua.</summary>
        public void MarkFirst(MatchMilestone milestone, float seconds)
        {
            var index = (int)milestone;
            if (_firsts[index] == NotReached)
            {
                _firsts[index] = seconds;
            }
        }

        public void Increment(MatchCounter counter) => _counts[(int)counter]++;

        /// <summary>Cộng thời gian cho trạng thái; bỏ qua giá trị không dương.</summary>
        public void AddTime(ThiefStateTime state, float seconds)
        {
            if (seconds > 0f)
            {
                _times[(int)state] += seconds;
            }
        }

        public float GetFirst(MatchMilestone milestone) => _firsts[(int)milestone];

        public int GetCount(MatchCounter counter) => _counts[(int)counter];

        public MatchLogEntry Build(MatchOutcome outcome, float durationSeconds, string configHash, DateTime endedAtUtc) =>
            new MatchLogEntry
            {
                EndedAtUtc = endedAtUtc.ToString("o", CultureInfo.InvariantCulture),
                Outcome = outcome.ToString(),
                DurationSeconds = durationSeconds,
                ConfigHash = configHash,
                FirstBarkSeconds = GetFirst(MatchMilestone.FirstBark),
                FirstOwnerWakeSeconds = GetFirst(MatchMilestone.OwnerWake),
                FirstSpottedSeconds = GetFirst(MatchMilestone.Spotted),
                FirstLureThrownSeconds = GetFirst(MatchMilestone.LureThrown),
                FirstDogLuredSeconds = GetFirst(MatchMilestone.DogLured),
                FirstPickupSeconds = GetFirst(MatchMilestone.Pickup),
                BarkCount = GetCount(MatchCounter.Bark),
                OwnerWakeCount = GetCount(MatchCounter.OwnerWake),
                SpottedCount = GetCount(MatchCounter.Spotted),
                LuresThrown = GetCount(MatchCounter.LureThrown),
                DogDropCount = GetCount(MatchCounter.DogDrop),
                SecondsInLight = _times[(int)ThiefStateTime.InLight],
                SecondsHidden = _times[(int)ThiefStateTime.Hidden],
                SecondsCrouching = _times[(int)ThiefStateTime.Crouching],
                SecondsSprinting = _times[(int)ThiefStateTime.Sprinting],
            };

        private static float[] CreateFirsts()
        {
            var firsts = new float[Enum.GetValues(typeof(MatchMilestone)).Length];
            for (var i = 0; i < firsts.Length; i++)
            {
                firsts[i] = NotReached;
            }

            return firsts;
        }
    }
}
