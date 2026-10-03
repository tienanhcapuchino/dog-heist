using System;
using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>
    /// Một loại tiếng (ví dụ "chó sủa"): danh sách clip để chọn ngẫu nhiên, khoảng âm lượng và pitch.
    /// Đổi clip hay chỉnh âm lượng chỉ cần sửa asset, không đụng code.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioCue", menuName = "DogHeist/Audio Cue")]
    public sealed class AudioCue : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips = Array.Empty<AudioClip>();
        [SerializeField] private Vector2 _volumeRange = new Vector2(0.9f, 1f);
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.95f, 1.05f);

        [Tooltip("0 = tiếng 2D (nghe đều mọi nơi), 1 = tiếng 3D (nhỏ dần theo khoảng cách).")]
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 1f;
        [SerializeField, Min(1f)] private float _maxDistance = 25f;

        [NonSerialized] private int _lastIndex = -1;
        [NonSerialized] private bool _warnedEmpty;

        public bool HasClips => _clips != null && _clips.Length > 0;

        public float SpatialBlend => _spatialBlend;

        public float MaxDistance => _maxDistance;

        internal void SetClips(AudioClip[] clips)
        {
            _clips = clips ?? Array.Empty<AudioClip>();
            _lastIndex = -1;
        }

        /// <summary>
        /// Chọn clip (không lặp lại clip vừa phát khi có từ hai clip), âm lượng và pitch.
        /// Cue chưa có clip thì trả về clip null và chỉ cảnh báo lần đầu.
        /// </summary>
        /// <param name="pickIndex">Nhận số clip, trả chỉ số được chọn. Mặc định ngẫu nhiên.</param>
        /// <param name="random01">Trả số trong [0, 1] để chọn âm lượng và pitch. Mặc định ngẫu nhiên.</param>
        public CuePlayback Prepare(Func<int, int> pickIndex = null, Func<float> random01 = null)
        {
            if (!HasClips)
            {
                if (!_warnedEmpty)
                {
                    _warnedEmpty = true;
                    Debug.LogWarning($"[Audio] Cue '{name}' chưa có clip nên sẽ im lặng. Kéo file âm thanh vào ô Clips.", this);
                }

                return default;
            }

            pickIndex ??= clipCount => UnityEngine.Random.Range(0, clipCount);
            random01 ??= () => UnityEngine.Random.value;

            var count = _clips.Length;
            var index = Mathf.Clamp(pickIndex(count), 0, count - 1);
            if (count > 1 && index == _lastIndex)
            {
                index = (index + 1) % count;
            }

            _lastIndex = index;
            var volume = Mathf.Lerp(_volumeRange.x, _volumeRange.y, random01());
            var pitch = Mathf.Lerp(_pitchRange.x, _pitchRange.y, random01());
            return new CuePlayback(_clips[index], volume, pitch);
        }
    }
}
