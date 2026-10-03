using System;
using DogHeist.Core.Match;
using DogHeist.Core.MatchLog;
using DogHeist.Core.Stats;
using DogHeist.Gameplay.Awareness;
using DogHeist.Gameplay.Dog;
using DogHeist.Gameplay.Noise;
using DogHeist.Gameplay.Thief;
using UnityEngine;

namespace DogHeist.Gameplay.Match
{
    /// <summary>
    /// Nghe các sự kiện trong ván, gom vào MatchLogBuilder và ghi một dòng nhật ký khi ván kết thúc.
    /// Chỉ đọc, không đổi trạng thái ván. Tham chiếu nào trống thì bỏ qua phần đó.
    /// </summary>
    public sealed class MatchLogRecorder : MonoBehaviour
    {
        [SerializeField] private MatchManager _match;
        [SerializeField] private ThiefMotor _motor;
        [SerializeField] private ThiefVisibility _visibility;
        [SerializeField] private ThiefInteractor _interactor;
        [SerializeField] private CarryableDog _dog;

        [Tooltip("DogAI (component cài IAwarenessSource).")]
        [SerializeField] private MonoBehaviour _dogAwareness;

        [Tooltip("OwnerAI (component cài IAwarenessSource).")]
        [SerializeField] private MonoBehaviour _ownerAwareness;

        [Tooltip("Các config đưa vào mã băm để gom các ván cùng một bộ thông số.")]
        [SerializeField] private ScriptableObject[] _configs;

        private readonly MatchLogBuilder _builder = new MatchLogBuilder();
        private Func<float> _clock;
        private IAwarenessSource _dogSource;
        private IAwarenessSource _ownerSource;
        private AwarenessLevel _ownerLevel = AwarenessLevel.None;
        private bool _written;

        internal IMatchLogWriter Writer { get; set; }

        internal Func<float> Clock
        {
            get => _clock ?? DefaultClock;
            set => _clock = value;
        }

        private float DefaultClock() => _match != null ? _match.ElapsedSeconds : 0f;

        private void Awake() => Writer ??= new JsonLinesMatchLogWriter();

        private void OnEnable()
        {
            NoiseSystem.NoiseEmitted += HandleNoise;
            MatchEvents.ThiefSpotted += HandleSpotted;
            MatchEvents.MatchEnded += HandleMatchEnded;

            if (_interactor != null)
            {
                _interactor.LureThrown += HandleLureThrown;
            }

            if (_dog != null)
            {
                _dog.PickedUp += HandlePickedUp;
                _dog.Dropped += HandleDropped;
            }

            _dogSource = _dogAwareness != null ? AwarenessSourceResolver.Resolve(_dogAwareness, this) : null;
            if (_dogSource != null)
            {
                _dogSource.AwarenessChanged += HandleDogAwareness;
            }

            _ownerSource = _ownerAwareness != null ? AwarenessSourceResolver.Resolve(_ownerAwareness, this) : null;
            if (_ownerSource != null)
            {
                _ownerLevel = _ownerSource.Awareness;
                _ownerSource.AwarenessChanged += HandleOwnerAwareness;
            }
        }

        private void OnDisable()
        {
            NoiseSystem.NoiseEmitted -= HandleNoise;
            MatchEvents.ThiefSpotted -= HandleSpotted;
            MatchEvents.MatchEnded -= HandleMatchEnded;

            if (_interactor != null)
            {
                _interactor.LureThrown -= HandleLureThrown;
            }

            if (_dog != null)
            {
                _dog.PickedUp -= HandlePickedUp;
                _dog.Dropped -= HandleDropped;
            }

            if (_dogSource != null)
            {
                _dogSource.AwarenessChanged -= HandleDogAwareness;
                _dogSource = null;
            }

            if (_ownerSource != null)
            {
                _ownerSource.AwarenessChanged -= HandleOwnerAwareness;
                _ownerSource = null;
            }
        }

        private void Update()
        {
            if (_match != null && _match.State == MatchState.Playing)
            {
                Sample(Time.deltaTime);
            }
        }

        internal void SetOwnerLevelForTests(AwarenessLevel level) => _ownerLevel = level;

        internal void HandleNoise(NoiseEvent noise)
        {
            if (noise.Source != NoiseSource.Dog)
            {
                return;
            }

            _builder.MarkFirst(MatchMilestone.FirstBark, Clock());
            _builder.Increment(MatchCounter.Bark);
        }

        internal void HandleOwnerAwareness(AwarenessLevel next)
        {
            if (_ownerLevel == AwarenessLevel.Sleeping && next != AwarenessLevel.Sleeping)
            {
                _builder.MarkFirst(MatchMilestone.OwnerWake, Clock());
                _builder.Increment(MatchCounter.OwnerWake);
            }

            _ownerLevel = next;
        }

        internal void HandleDogAwareness(AwarenessLevel next)
        {
            if (next == AwarenessLevel.Friendly)
            {
                _builder.MarkFirst(MatchMilestone.DogLured, Clock());
            }
        }

        internal void HandleSpotted()
        {
            _builder.MarkFirst(MatchMilestone.Spotted, Clock());
            _builder.Increment(MatchCounter.Spotted);
        }

        internal void HandleLureThrown()
        {
            _builder.MarkFirst(MatchMilestone.LureThrown, Clock());
            _builder.Increment(MatchCounter.LureThrown);
        }

        internal void HandlePickedUp() => _builder.MarkFirst(MatchMilestone.Pickup, Clock());

        internal void HandleDropped() => _builder.Increment(MatchCounter.DogDrop);

        internal void Sample(float deltaTime)
        {
            if (_visibility != null)
            {
                if (_visibility.IsInLight)
                {
                    _builder.AddTime(ThiefStateTime.InLight, deltaTime);
                }

                if (_visibility.IsHidden)
                {
                    _builder.AddTime(ThiefStateTime.Hidden, deltaTime);
                }
            }

            if (_motor != null)
            {
                if (_motor.IsCrouching)
                {
                    _builder.AddTime(ThiefStateTime.Crouching, deltaTime);
                }

                if (_motor.IsSprinting)
                {
                    _builder.AddTime(ThiefStateTime.Sprinting, deltaTime);
                }
            }
        }

        internal void HandleMatchEnded(MatchResult result, PlayerStats stats)
        {
            if (_written)
            {
                return;
            }

            _written = true;
            Writer ??= new JsonLinesMatchLogWriter();
            Writer.Append(_builder.Build(result.Outcome, result.DurationSeconds, ComputeConfigHash(), DateTime.UtcNow));
        }

        private string ComputeConfigHash()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (_configs != null)
            {
                foreach (var config in _configs)
                {
                    if (config != null)
                    {
                        parts.Add(JsonUtility.ToJson(config));
                    }
                }
            }

            return ConfigFingerprint.Compute(parts);
        }
    }
}
