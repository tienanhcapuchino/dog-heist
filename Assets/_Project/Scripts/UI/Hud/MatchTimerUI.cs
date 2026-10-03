using DogHeist.Gameplay.Match;
using TMPro;
using UnityEngine;

namespace DogHeist.UI.Hud
{
    /// <summary>Đồng hồ ván trên HUD (phút:giây). Chỉ đọc MatchManager.ElapsedSeconds.</summary>
    public sealed class MatchTimerUI : MonoBehaviour
    {
        [SerializeField] private MatchManager _match;
        [SerializeField] private TMP_Text _label;

        private int _shownSeconds = -1;

        private void Update()
        {
            if (_match == null || _label == null)
            {
                return;
            }

            // Chỉ cập nhật chữ khi giây nguyên đổi, tránh tạo chuỗi mới mỗi khung hình.
            var whole = Mathf.FloorToInt(Mathf.Max(0f, _match.ElapsedSeconds));
            if (whole == _shownSeconds)
            {
                return;
            }

            _shownSeconds = whole;
            _label.text = FormatTime(whole);
        }

        /// <summary>Đổi số giây thành "phút:giây", làm tròn xuống; số âm coi như 0.</summary>
        internal static string FormatTime(float seconds)
        {
            var whole = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            return $"{whole / 60}:{whole % 60:00}";
        }
    }
}
