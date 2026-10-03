using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.UI;

namespace DogHeist.UI.Hud
{
    /// <summary>
    /// Con mắt trên HUD cho biết trộm đang lộ tới đâu: nấp trong bụi chỉ còn một khe,
    /// trong tối mở hé, dưới đèn mở to và đổi màu vàng. Chỉ đọc ThiefVisibility.
    /// </summary>
    public sealed class VisibilityEyeUI : MonoBehaviour
    {
        [SerializeField] private ThiefVisibility _visibility;

        [Tooltip("Lòng trắng của mắt; độ mở là độ cao (scale Y) của nó.")]
        [SerializeField] private RectTransform _eyeWhite;
        [SerializeField] private Image _eyeWhiteImage;

        [SerializeField, Range(0f, 1f)] private float _minOpenness = 0.08f;
        [SerializeField, Range(0f, 1f)] private float _maxOpenness = 1f;
        [SerializeField] private Color _darkColor = Color.white;
        [SerializeField] private Color _litColor = new Color(1f, 0.85f, 0.2f);

        private void Update()
        {
            if (_visibility == null)
            {
                return;
            }

            if (_eyeWhite != null)
            {
                var scale = _eyeWhite.localScale;
                scale.y = ComputeOpenness(_visibility.Visibility01, _minOpenness, _maxOpenness);
                _eyeWhite.localScale = scale;
            }

            if (_eyeWhiteImage != null)
            {
                _eyeWhiteImage.color = _visibility.IsInLight ? _litColor : _darkColor;
            }
        }

        /// <summary>Đổi độ lộ diện (0..1) thành độ mở của mắt trong khoảng [min, max].</summary>
        internal static float ComputeOpenness(float visibility01, float minOpenness, float maxOpenness) =>
            Mathf.Lerp(minOpenness, maxOpenness, Mathf.Clamp01(visibility01));
    }
}
