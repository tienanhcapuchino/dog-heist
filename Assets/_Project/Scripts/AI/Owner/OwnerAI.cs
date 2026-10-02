using System;
using System.Collections.Generic;
using DogHeist.AI.Sensors;
using DogHeist.Core.FSM;
using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Noise;
using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.AI;

namespace DogHeist.AI.Owner
{
    /// <summary>
    /// Bộ não chủ nhà (do máy điều khiển ở bản chơi đơn). Các trạng thái nằm trong OwnerStates.cs:
    /// Sleeping (ngủ) → Investigate (ra xem) → Patrol (đi tuần) → Chase (đuổi bắt).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class OwnerAI : MonoBehaviour
    {
        [SerializeField] private OwnerConfig _config;
        [SerializeField] private HearingSensor _hearing;
        [SerializeField] private VisionSensor _vision;

        [Tooltip("Bản chơi đơn chỉ có một trộm. Bản multiplayer sẽ thay bằng danh sách trộm.")]
        [SerializeField] private ThiefVisibility _thief;

        [Tooltip("Chỗ ngủ (ví dụ võng ngoài hiên). Để trống sẽ dùng vị trí ban đầu.")]
        [SerializeField] private Transform _bed;

        [SerializeField] private Transform[] _patrolWaypoints = Array.Empty<Transform>();

        private readonly StateMachine _stateMachine = new();
        private NavMeshAgent _agent;
        private Vector3 _spawnPosition;

        public string CurrentStateName => _stateMachine.CurrentState?.GetType().Name ?? "None";

        internal OwnerConfig Config => _config;

        internal ThiefVisibility Thief => _thief;

        internal Vector3 BedPosition => _bed != null ? _bed.position : _spawnPosition;

        internal IReadOnlyList<Transform> PatrolWaypoints => _patrolWaypoints;

        internal OwnerSleepingState SleepingState { get; private set; }

        internal OwnerInvestigateState InvestigateState { get; private set; }

        internal OwnerPatrolState PatrolState { get; private set; }

        internal OwnerChaseState ChaseState { get; private set; }

        private OwnerState CurrentState => _stateMachine.CurrentState as OwnerState;

        private bool CanUseAgent => _agent.enabled && _agent.isOnNavMesh;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _spawnPosition = transform.position;

            if (_config == null)
            {
                Debug.LogError($"{name}: chưa gán OwnerConfig cho OwnerAI.", this);
                enabled = false;
                return;
            }

            SleepingState = new OwnerSleepingState(this);
            InvestigateState = new OwnerInvestigateState(this);
            PatrolState = new OwnerPatrolState(this);
            ChaseState = new OwnerChaseState(this);
        }

        private void OnEnable()
        {
            if (_hearing != null)
            {
                _hearing.NoiseHeard += HandleNoiseHeard;
            }

            MatchEvents.MatchEnded += HandleMatchEnded;
        }

        private void OnDisable()
        {
            if (_hearing != null)
            {
                _hearing.NoiseHeard -= HandleNoiseHeard;
            }

            MatchEvents.MatchEnded -= HandleMatchEnded;
        }

        private void Start() => ChangeState(SleepingState);

        private void Update() => _stateMachine.Tick(Time.deltaTime);

        internal void ChangeState(OwnerState next) => _stateMachine.ChangeState(next);

        internal void Investigate(Vector3 point)
        {
            InvestigateState.SetTarget(point);
            ChangeState(InvestigateState);
        }

        internal void SetHearingSensitivity(float sensitivity)
        {
            if (_hearing != null)
            {
                _hearing.Sensitivity = sensitivity;
            }
        }

        internal bool CanSeeThief() => _vision != null && _vision.CanSee(_thief);

        internal bool IsWithinCatchDistance()
        {
            if (_thief == null)
            {
                return false;
            }

            var offset = _thief.transform.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= _config.CatchDistance * _config.CatchDistance;
        }

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

        internal bool HasArrived()
        {
            if (!CanUseAgent || _agent.pathPending)
            {
                return false;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
        }

        internal void LookAround(float deltaTime) => transform.Rotate(0f, _config.LookAroundSpeed * deltaTime, 0f);

        private void HandleNoiseHeard(NoiseEvent noise) => CurrentState?.OnNoiseHeard(noise);

        private void HandleMatchEnded(MatchResult result, PlayerStats stats)
        {
            Stop();
            enabled = false;
        }
    }
}
