using DogHeist.Gameplay.Thief;
using TMPro;
using UnityEngine;

namespace DogHeist.UI.Hud
{
    /// <summary>
    /// Thông tin trên HUD: gợi ý phím tương tác, số đồ ăn còn lại, trạng thái ẩn nấp.
    /// </summary>
    public sealed class ThiefStatusUI : MonoBehaviour
    {
        [SerializeField] private ThiefInteractor _interactor;
        [SerializeField] private ThiefVisibility _visibility;
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private TMP_Text _lureText;
        [SerializeField] private TMP_Text _visibilityText;
        [SerializeField] private string _interactKeyLabel = "E";

        private void Update()
        {
            if (_interactor != null)
            {
                SetText(_promptText, BuildPrompt());
                SetText(_lureText, $"Đồ ăn dụ chó: {_interactor.LuresRemaining}");
            }

            if (_visibility != null)
            {
                SetText(_visibilityText, BuildVisibilityLabel());
            }
        }

        private string BuildPrompt()
        {
            if (_interactor.IsCarrying)
            {
                return $"[{_interactKeyLabel}] Thả chó   |   Mang chó ra khỏi cổng!";
            }

            var focused = _interactor.FocusedInteractable;
            return focused != null ? $"[{_interactKeyLabel}] {focused.Prompt}" : string.Empty;
        }

        private string BuildVisibilityLabel()
        {
            if (_visibility.IsHidden)
            {
                return "Đang ẩn nấp";
            }

            return _visibility.IsInLight ? "Đang bị lộ!" : "Trong bóng tối";
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null && label.text != value)
            {
                label.text = value;
            }
        }
    }
}
