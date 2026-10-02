using System.Text;
using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using DogHeist.Gameplay.Match;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogHeist.UI.Screens
{
    /// <summary>
    /// Màn hình kết quả cuối ván kèm bảng thành tích.
    /// Gắn script lên một GameObject luôn bật (ví dụ Canvas), KHÔNG gắn lên chính panel kết quả,
    /// vì panel bị ẩn lúc đầu sẽ không nhận được sự kiện.
    /// </summary>
    public sealed class ResultScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _detailsText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private MatchManager _matchManager;

        private void Awake()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        private void OnEnable() => MatchEvents.MatchEnded += Show;

        private void OnDisable() => MatchEvents.MatchEnded -= Show;

        private void OnDestroy()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }

        private void Show(MatchResult result, PlayerStats stats)
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
            }

            if (_titleText != null)
            {
                _titleText.text = BuildTitle(result);
            }

            if (_detailsText != null)
            {
                _detailsText.text = BuildDetails(result, stats);
            }
        }

        private static string BuildTitle(MatchResult result)
        {
            if (!result.LocalPlayerWon)
            {
                return "Bị bắt rồi!";
            }

            return result.WasThiefSpotted ? "Trộm thành công!" : "Trộm hoàn hảo, không ai phát hiện!";
        }

        private static string BuildDetails(MatchResult result, PlayerStats stats)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Thời gian ván này: {FormatDuration(result.DurationSeconds)}");
            builder.AppendLine();
            builder.AppendLine($"Số lần trộm thành công: {stats.SuccessfulHeists}");
            builder.AppendLine($"Trong đó trộm hoàn hảo: {stats.CleanHeists}");
            builder.AppendLine($"Chuỗi thắng hiện tại: {stats.CurrentHeistStreak}");
            builder.AppendLine($"Chuỗi thắng cao nhất: {stats.BestHeistStreak}");
            builder.Append($"Số lần bị bắt: {stats.TimesCaught}");
            return builder.ToString();
        }

        private static string FormatDuration(float seconds)
        {
            var totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }

        private void HandleRestartClicked()
        {
            if (_matchManager != null)
            {
                _matchManager.RestartMatch();
            }
        }
    }
}
