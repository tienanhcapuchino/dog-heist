using DogHeist.Gameplay.Awareness;
using TMPro;
using UnityEngine;

namespace DogHeist.UI.Awareness
{
    /// <summary>
    /// Dấu chữ 3D trên đầu chó hoặc chủ nhà (Zzz, ?, !, ♥). Chỉ đọc IAwarenessSource, không biết gì về AI.
    /// Luôn quay mặt về camera và "nảy" lên một chút mỗi khi mức đổi.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public sealed class AwarenessIndicatorUI : MonoBehaviour
    {
        [Tooltip("DogAI hoặc OwnerAI (component cài IAwarenessSource).")]
        [SerializeField] private MonoBehaviour _source;
        [SerializeField] private AwarenessIndicatorStyle _style;

        [Tooltip("Độ phóng to lúc nảy khi mức đổi.")]
        [SerializeField, Min(0f)] private float _popScale = 1.4f;
        [SerializeField, Min(0.01f)] private float _popDuration = 0.2f;

        private IAwarenessSource _bound;
        private TextMeshPro _label;
        private Vector3 _baseScale;
        private bool _hasBaseScale;
        private float _popTimer;

        // Lấy khi cần thay vì trong Awake, vì EditMode test không gọi Awake.
        private TextMeshPro Label => _label != null ? _label : _label = GetComponent<TextMeshPro>();

        private void OnEnable()
        {
            if (_style == null)
            {
                Debug.LogError("[Awareness] Chưa gán AwarenessIndicatorStyle cho dấu cảnh giác.", this);
                enabled = false;
                return;
            }

            if (!Bind(AwarenessSourceResolver.Resolve(_source, this)))
            {
                enabled = false;
            }
        }

        private void OnDisable() => Unbind();

        private void LateUpdate()
        {
            if (_bound == null)
            {
                return;
            }

            var anchor = _bound.IndicatorAnchor;
            if (anchor != null)
            {
                transform.position = anchor.position;
            }

            var camera = Camera.main;
            if (camera != null)
            {
                // Cùng hướng với camera để chữ luôn đọc được, không bị lật hay méo.
                transform.rotation = camera.transform.rotation;
            }

            if (_popTimer > 0f)
            {
                _popTimer = Mathf.Max(0f, _popTimer - Time.deltaTime);
                var factor = Mathf.Lerp(1f, _popScale, _popTimer / _popDuration);
                transform.localScale = _baseScale * factor;
            }
        }

        /// <summary>Đăng ký nghe nguồn và hiện ngay mức hiện tại.</summary>
        /// <returns>false khi không có nguồn.</returns>
        internal bool Bind(IAwarenessSource source)
        {
            Unbind();
            if (source == null)
            {
                return false;
            }

            if (!_hasBaseScale)
            {
                _baseScale = transform.localScale;
                _hasBaseScale = true;
            }

            _bound = source;
            _bound.AwarenessChanged += Apply;
            Apply(_bound.Awareness);
            return true;
        }

        /// <summary>Đổi chữ và màu theo mức; mức không có dấu (None) thì ẩn.</summary>
        internal void Apply(AwarenessLevel level)
        {
            var label = Label;
            if (_style == null || !_style.TryGet(level, out var text, out var color))
            {
                label.enabled = false;
                return;
            }

            label.enabled = true;
            label.text = text;
            label.color = color;
            _popTimer = _popDuration;
        }

        private void Unbind()
        {
            if (_bound != null)
            {
                _bound.AwarenessChanged -= Apply;
                _bound = null;
            }

            if (_hasBaseScale)
            {
                transform.localScale = _baseScale;
                _popTimer = 0f;
            }
        }
    }
}
