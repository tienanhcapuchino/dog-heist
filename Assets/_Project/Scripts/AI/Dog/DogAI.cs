using System;
using DogHeist.AI.Sensors;
using DogHeist.Core.FSM;
using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using DogHeist.Gameplay.Awareness;
using DogHeist.Gameplay.Dog;
using DogHeist.Gameplay.Items;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Noise;
using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.AI;

namespace DogHeist.AI.Dog
{
    /// <summary>
    /// Bộ não con chó. Các trạng thái nằm trong DogStates.cs:
    /// Idle (lang thang) → Alert (sủa) → EatLure (ăn đồ dụ) → Calm (hiền, cho bế) → Carried (bị bế).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(CarryableDog))]
    public sealed class DogAI : MonoBehaviour, IAwarenessSource
    {
        private const float NavMeshSnapDistance = 2f;
        private const int WanderSampleAttempts = 5;

        [SerializeField] private DogConfig _config;
        [SerializeField] private HearingSensor _hearing;

        [Tooltip("Bản chơi đơn chỉ có một trộm. Bản multiplayer sẽ thay bằng danh sách trộm.")]
        [SerializeField] private ThiefVisibility _thief;

        [Tooltip("Vị trí chuồng chó. Để trống sẽ dùng vị trí ban đầu.")]
        [SerializeField] private Transform _home;

        [Tooltip("Điểm trên đầu để đặt dấu cảnh giác. Để trống sẽ dùng chính con chó.")]
        [SerializeField] private Transform _indicatorAnchor;

        private readonly StateMachine _stateMachine = new();
        private readonly AwarenessTracker _awareness = new();
        private NavMeshAgent _agent;
        private CarryableDog _carryable;
        private Vector3 _spawnPosition;

        public event Action<AwarenessLevel> AwarenessChanged
        {
            add => _awareness.Changed += value;
            remove => _awareness.Changed -= value;
        }

        public string CurrentStateName => _stateMachine.CurrentState?.GetType().Name ?? "None";

        public AwarenessLevel Awareness => _awareness.Current;

        public Transform IndicatorAnchor => _indicatorAnchor != null ? _indicatorAnchor : transform;

        internal DogConfig Config => _config;

        internal CarryableDog Carryable => _carryable;

        internal ThiefVisibility Thief => _thief;

        internal Vector3 HomePosition => _home != null ? _home.position : _spawnPosition;

        internal DogIdleState IdleState { get; private set; }

        internal DogAlertState AlertState { get; private set; }

        internal DogEatLureState EatLureState { get; private set; }

        internal DogCalmState CalmState { get; private set; }

        internal DogCarriedState CarriedState { get; private set; }

        private DogState CurrentState => _stateMachine.CurrentState as DogState;

        private bool CanUseAgent => _agent.enabled && _agent.isOnNavMesh;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _carryable = GetComponent<CarryableDog>();
            _spawnPosition = transform.position;

            if (_config == null)
            {
                Debug.LogError($"{name}: chưa gán DogConfig cho DogAI.", this);
                enabled = false;
                return;
            }

            IdleState = new DogIdleState(this);
            AlertState = new DogAlertState(this);
            EatLureState = new DogEatLureState(this);
            CalmState = new DogCalmState(this);
            CarriedState = new DogCarriedState(this);
            _stateMachine.StateChanged += HandleStateChanged;
        }

        private void OnEnable()
        {
            if (_hearing != null)
            {
                _hearing.NoiseHeard += HandleNoiseHeard;
            }

            _carryable.PickedUp += HandlePickedUp;
            _carryable.Dropped += HandleDropped;
            MatchEvents.MatchEnded += HandleMatchEnded;
        }

        private void OnDisable()
        {
            if (_hearing != null)
            {
                _hearing.NoiseHeard -= HandleNoiseHeard;
            }

            _carryable.PickedUp -= HandlePickedUp;
            _carryable.Dropped -= HandleDropped;
            MatchEvents.MatchEnded -= HandleMatchEnded;
        }

        private void Start() => ChangeState(IdleState);

        private void Update() => _stateMachine.Tick(Time.deltaTime);

        internal void ChangeState(DogState next) => _stateMachine.ChangeState(next);

        internal void MoveTo(Vector3 destination, float speed)
        {
            if (!CanUseAgent)
            {
                return;
            }

            _agent.speed = speed;
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        internal void Stop()
        {
            if (!CanUseAgent)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        internal void SetAgentActive(bool active)
        {
            if (!active)
            {
                Stop();
                _agent.enabled = false;
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out var hit, NavMeshSnapDistance, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }

            _agent.enabled = true;
        }

        internal void FaceTowards(Vector3 point, float deltaTime)
        {
            var direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(direction), _config.TurnSpeed * deltaTime);
        }

        internal bool CanNoticeThief()
        {
            if (_thief == null || !_thief.isActiveAndEnabled)
            {
                return false;
            }

            var noticeDistance = _config.NoticeDistance * _thief.Visibility01;
            return (_thief.transform.position - transform.position).sqrMagnitude <= noticeDistance * noticeDistance;
        }

        internal float FlatDistanceTo(Vector3 point)
        {
            var offset = point - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        internal void Bark() =>
            NoiseSystem.Emit(new NoiseEvent(transform.position, _config.BarkNoiseRadius, NoiseSource.Dog, gameObject));

        internal FoodLure FindAvailableLure() => FoodLure.FindNearestUnclaimed(transform.position);

        internal bool TryGetWanderPoint(out Vector3 point)
        {
            for (var attempt = 0; attempt < WanderSampleAttempts; attempt++)
            {
                var candidate = HomePosition + UnityEngine.Random.insideUnitSphere * _config.WanderRadius;
                if (NavMesh.SamplePosition(candidate, out var hit, NavMeshSnapDistance, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = HomePosition;
            return false;
        }

        // Đọc trạng thái hiện tại thay vì tham số "next": nếu Enter của trạng thái mới lại đổi trạng thái,
        // sự kiện của lần đổi ngoài phát sau cùng và tham số "next" khi đó đã cũ.
        private void HandleStateChanged(IState previous, IState next)
        {
            if (CurrentState != null)
            {
                _awareness.Set(CurrentState.Awareness);
            }
        }

        private void HandleNoiseHeard(NoiseEvent noise) => CurrentState?.OnNoiseHeard(noise);

        private void HandlePickedUp() => ChangeState(CarriedState);

        private void HandleDropped() => CurrentState?.OnDropped();

        private void HandleMatchEnded(MatchResult result, PlayerStats stats)
        {
            Stop();
            enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.4f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, _config.NoticeDistance);
        }
    }
}
