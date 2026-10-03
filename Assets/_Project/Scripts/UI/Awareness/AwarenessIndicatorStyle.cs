using DogHeist.Gameplay.Awareness;
using UnityEngine;

namespace DogHeist.UI.Awareness
{
    /// <summary>
    /// Chữ và màu của dấu trên đầu AI theo từng mức cảnh giác. Đây là thông số trình bày, không phải thông số cân bằng.
    /// </summary>
    [CreateAssetMenu(fileName = "AwarenessIndicatorStyle", menuName = "DogHeist/Awareness Indicator Style")]
    public sealed class AwarenessIndicatorStyle : ScriptableObject
    {
        [Header("Ngủ")]
        [SerializeField] private string _sleepingText = "Zzz";
        [SerializeField] private Color _sleepingColor = new Color(0.75f, 0.75f, 0.8f);

        [Header("Nghi ngờ")]
        [SerializeField] private string _suspiciousText = "?";
        [SerializeField] private Color _suspiciousColor = new Color(1f, 0.85f, 0.2f);

        [Header("Phát hiện")]
        [SerializeField] private string _alertedText = "!";
        [SerializeField] private Color _alertedColor = new Color(1f, 0.25f, 0.2f);

        [Header("Thân thiện")]
        [SerializeField] private string _friendlyText = "♥";
        [SerializeField] private Color _friendlyColor = new Color(1f, 0.45f, 0.65f);

        /// <returns>false khi mức này không hiện dấu (None).</returns>
        public bool TryGet(AwarenessLevel level, out string text, out Color color)
        {
            switch (level)
            {
                case AwarenessLevel.Sleeping:
                    text = _sleepingText;
                    color = _sleepingColor;
                    return true;
                case AwarenessLevel.Suspicious:
                    text = _suspiciousText;
                    color = _suspiciousColor;
                    return true;
                case AwarenessLevel.Alerted:
                    text = _alertedText;
                    color = _alertedColor;
                    return true;
                case AwarenessLevel.Friendly:
                    text = _friendlyText;
                    color = _friendlyColor;
                    return true;
                default:
                    text = string.Empty;
                    color = Color.clear;
                    return false;
            }
        }
    }
}
