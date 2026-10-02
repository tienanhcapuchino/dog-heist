using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.UI;

namespace DogHeist.UI.Hud
{
    /// <summary>
    /// Thanh tiếng ồn: cho người chơi biết mình đang ồn tới mức nào.
    /// Image cần đặt Image Type = Filled.
    /// </summary>
    public sealed class NoiseMeterUI : MonoBehaviour
    {
        [SerializeField] private ThiefMotor _thief;
        [SerializeField] private Image _fill;
        [SerializeField] private Gradient _colorByLevel = new();
        [SerializeField, Min(0.1f)] private float _smoothing = 8f;

        private float _displayedLevel;

        private void Reset()
        {
            _colorByLevel = new Gradient();
            _colorByLevel.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.30f, 0.80f, 0.45f), 0f),
                    new GradientColorKey(new Color(0.95f, 0.80f, 0.20f), 0.5f),
                    new GradientColorKey(new Color(0.90f, 0.30f, 0.25f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
        }

        private void Update()
        {
            if (_thief == null || _fill == null)
            {
                return;
            }

            var blend = 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
            _displayedLevel = Mathf.Lerp(_displayedLevel, _thief.NoiseLevel01, blend);
            _fill.fillAmount = _displayedLevel;
            _fill.color = _colorByLevel.Evaluate(_displayedLevel);
        }
    }
}
