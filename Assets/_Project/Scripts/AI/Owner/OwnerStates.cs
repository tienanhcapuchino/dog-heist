using DogHeist.Core.FSM;
using DogHeist.Gameplay.Awareness;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Noise;
using UnityEngine;

namespace DogHeist.AI.Owner
{
    internal abstract class OwnerState : IState
    {
        protected OwnerState(OwnerAI owner) => Owner = owner;

        protected OwnerAI Owner { get; }

        /// <summary>Mức cảnh giác hiện ra trên đầu chủ nhà khi ở trạng thái này.</summary>
        public abstract AwarenessLevel Awareness { get; }

        public virtual void Enter()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void OnNoiseHeard(NoiseEvent noise) => Owner.Investigate(noise.Position);

        protected bool TrySpotThief()
        {
            if (!Owner.CanSeeThief())
            {
                return false;
            }

            Owner.ChangeState(Owner.ChaseState);
            return true;
        }
    }

    /// <summary>Đi về chỗ ngủ rồi ngủ. Lúc ngủ nghe kém và không nhìn thấy gì.</summary>
    internal sealed class OwnerSleepingState : OwnerState
    {
        public OwnerSleepingState(OwnerAI owner) : base(owner)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Sleeping;

        public override void Enter()
        {
            Owner.SetHearingSensitivity(Owner.Config.SleepingHearingSensitivity);
            Owner.MoveTo(Owner.BedPosition, Owner.Config.WalkSpeed);
        }

        public override void Tick(float deltaTime)
        {
            // Trên đường về giường vẫn còn thức nên vẫn nhìn thấy trộm.
            if (!Owner.HasArrived())
            {
                TrySpotThief();
            }
        }

        public override void Exit() => Owner.SetHearingSensitivity(Owner.Config.AwakeHearingSensitivity);
    }

    /// <summary>Đi tới chỗ phát ra tiếng động, đứng nhìn quanh, rồi chuyển sang đi tuần.</summary>
    internal sealed class OwnerInvestigateState : OwnerState
    {
        private Vector3 _target;
        private bool _hasArrived;
        private bool _isActive;
        private float _lookTimer;

        public OwnerInvestigateState(OwnerAI owner) : base(owner)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Suspicious;

        public void SetTarget(Vector3 target)
        {
            _target = target;
            _hasArrived = false;

            if (_isActive)
            {
                Owner.MoveTo(_target, Owner.Config.InvestigateSpeed);
            }
        }

        public override void Enter()
        {
            _isActive = true;
            _hasArrived = false;
            Owner.MoveTo(_target, Owner.Config.InvestigateSpeed);
        }

        public override void Tick(float deltaTime)
        {
            if (TrySpotThief())
            {
                return;
            }

            if (!_hasArrived)
            {
                if (Owner.HasArrived())
                {
                    _hasArrived = true;
                    _lookTimer = Owner.Config.InvestigateLookTime;
                    Owner.Stop();
                }

                return;
            }

            Owner.LookAround(deltaTime);
            _lookTimer -= deltaTime;
            if (_lookTimer <= 0f)
            {
                Owner.ChangeState(Owner.PatrolState);
            }
        }

        public override void Exit() => _isActive = false;
    }

    /// <summary>Đi tuần qua các điểm đặt sẵn. Hết thời gian mà không có gì thì quay lại ngủ.</summary>
    internal sealed class OwnerPatrolState : OwnerState
    {
        private int _waypointIndex;
        private bool _isWaiting;
        private float _waitTimer;
        private float _patrolTimer;

        public OwnerPatrolState(OwnerAI owner) : base(owner)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Suspicious;

        public override void Enter()
        {
            _patrolTimer = Owner.Config.PatrolDuration;
            _isWaiting = false;
            MoveToCurrentWaypoint();
        }

        public override void Tick(float deltaTime)
        {
            if (TrySpotThief())
            {
                return;
            }

            _patrolTimer -= deltaTime;
            if (_patrolTimer <= 0f || Owner.PatrolWaypoints.Count == 0)
            {
                Owner.ChangeState(Owner.SleepingState);
                return;
            }

            if (_isWaiting)
            {
                Owner.LookAround(deltaTime * 0.5f);
                _waitTimer -= deltaTime;
                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    _waypointIndex = (_waypointIndex + 1) % Owner.PatrolWaypoints.Count;
                    MoveToCurrentWaypoint();
                }

                return;
            }

            if (Owner.HasArrived())
            {
                _isWaiting = true;
                _waitTimer = Owner.Config.WaypointWaitTime;
            }
        }

        private void MoveToCurrentWaypoint()
        {
            var waypoints = Owner.PatrolWaypoints;
            if (waypoints.Count == 0)
            {
                return;
            }

            _waypointIndex %= waypoints.Count;
            var waypoint = waypoints[_waypointIndex];
            if (waypoint != null)
            {
                Owner.MoveTo(waypoint.position, Owner.Config.WalkSpeed);
            }
        }
    }

    /// <summary>Đuổi theo trộm. Tới đủ gần thì bắt; mất dấu quá lâu thì tới chỗ thấy trộm lần cuối để tìm.</summary>
    internal sealed class OwnerChaseState : OwnerState
    {
        private Vector3 _lastKnownPosition;
        private float _lostSightTimer;
        private float _repathTimer;

        public OwnerChaseState(OwnerAI owner) : base(owner)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Alerted;

        public override void Enter()
        {
            _lastKnownPosition = Owner.Thief.transform.position;
            _lostSightTimer = 0f;
            _repathTimer = 0f;
            MatchEvents.RaiseThiefSpotted();
        }

        public override void Tick(float deltaTime)
        {
            if (Owner.CanSeeThief())
            {
                _lastKnownPosition = Owner.Thief.transform.position;
                _lostSightTimer = 0f;
            }
            else
            {
                _lostSightTimer += deltaTime;
                if (_lostSightTimer >= Owner.Config.LoseSightTime)
                {
                    Owner.Investigate(_lastKnownPosition);
                    return;
                }
            }

            if (Owner.IsWithinCatchDistance())
            {
                Owner.Stop();
                MatchEvents.RaiseThiefCaught();
                return;
            }

            _repathTimer -= deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = Owner.Config.ChaseRepathInterval;
                Owner.MoveTo(_lastKnownPosition, Owner.Config.RunSpeed);
            }
        }

        public override void OnNoiseHeard(NoiseEvent noise)
        {
            // Đang đuổi thì không bỏ đi xem tiếng khác, chỉ dùng tiếng bước chân để đoán vị trí trộm.
            if (noise.Source == NoiseSource.Thief)
            {
                _lastKnownPosition = noise.Position;
            }
        }
    }
}
