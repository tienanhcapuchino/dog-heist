using DogHeist.Core.FSM;
using DogHeist.Gameplay.Awareness;
using DogHeist.Gameplay.Items;
using DogHeist.Gameplay.Noise;
using UnityEngine;

namespace DogHeist.AI.Dog
{
    internal abstract class DogState : IState
    {
        protected DogState(DogAI dog) => Dog = dog;

        protected DogAI Dog { get; }

        /// <summary>Mức cảnh giác hiện ra trên đầu chó khi ở trạng thái này.</summary>
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

        public virtual void OnNoiseHeard(NoiseEvent noise)
        {
        }

        public virtual void OnDropped()
        {
        }

        /// <summary>Đồ ăn luôn thắng: kể cả đang sủa, chó vẫn bị đồ ăn dụ đi. Đây là "chìa khóa" của vai trộm.</summary>
        protected bool TryGoForLure()
        {
            var lure = Dog.FindAvailableLure();
            if (lure == null || !lure.TryClaim())
            {
                return false;
            }

            Dog.EatLureState.SetTarget(lure);
            Dog.ChangeState(Dog.EatLureState);
            return true;
        }

        protected void BecomeAlert(Vector3 focusPoint)
        {
            Dog.AlertState.SetFocus(focusPoint);
            Dog.ChangeState(Dog.AlertState);
        }
    }

    /// <summary>Lang thang quanh chuồng. Phát hiện trộm hoặc nghe tiếng trộm thì chuyển sang sủa.</summary>
    internal sealed class DogIdleState : DogState
    {
        private float _wanderTimer;

        public DogIdleState(DogAI dog) : base(dog)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.None;

        public override void Enter()
        {
            Dog.Carryable.AllowPickup = false;
            _wanderTimer = 0f;
        }

        public override void Tick(float deltaTime)
        {
            if (TryGoForLure())
            {
                return;
            }

            if (Dog.CanNoticeThief())
            {
                BecomeAlert(Dog.Thief.transform.position);
                return;
            }

            _wanderTimer -= deltaTime;
            if (_wanderTimer > 0f)
            {
                return;
            }

            _wanderTimer = Dog.Config.WanderInterval;
            if (Dog.TryGetWanderPoint(out var point))
            {
                Dog.MoveTo(point, Dog.Config.WalkSpeed);
            }
        }

        public override void OnNoiseHeard(NoiseEvent noise)
        {
            if (noise.Source == NoiseSource.Thief)
            {
                BecomeAlert(noise.Position);
            }
        }
    }

    /// <summary>Đứng sủa về phía trộm. Tiếng sủa là tiếng ồn lớn, dễ đánh thức chủ nhà.</summary>
    internal sealed class DogAlertState : DogState
    {
        private Vector3 _focusPoint;
        private float _barkTimer;
        private float _calmDownTimer;

        public DogAlertState(DogAI dog) : base(dog)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Alerted;

        public void SetFocus(Vector3 focusPoint) => _focusPoint = focusPoint;

        public override void Enter()
        {
            Dog.Carryable.AllowPickup = false;
            Dog.Stop();
            _barkTimer = 0f;
            _calmDownTimer = Dog.Config.AlertDuration;
        }

        public override void Tick(float deltaTime)
        {
            if (TryGoForLure())
            {
                return;
            }

            if (Dog.CanNoticeThief())
            {
                _focusPoint = Dog.Thief.transform.position;
                _calmDownTimer = Dog.Config.AlertDuration;
            }
            else
            {
                _calmDownTimer -= deltaTime;
                if (_calmDownTimer <= 0f)
                {
                    Dog.ChangeState(Dog.IdleState);
                    return;
                }
            }

            Dog.FaceTowards(_focusPoint, deltaTime);

            _barkTimer -= deltaTime;
            if (_barkTimer <= 0f)
            {
                _barkTimer = Dog.Config.BarkInterval;
                Dog.Bark();
            }
        }

        public override void OnNoiseHeard(NoiseEvent noise)
        {
            if (noise.Source != NoiseSource.Thief)
            {
                return;
            }

            _focusPoint = noise.Position;
            _calmDownTimer = Dog.Config.AlertDuration;
        }
    }

    /// <summary>Chạy tới đồ ăn và ăn. Trong lúc ăn, trộm có thể bế chó đi.</summary>
    internal sealed class DogEatLureState : DogState
    {
        private const float RepathInterval = 0.25f;

        private FoodLure _lure;
        private bool _isEating;
        private float _eatTimer;
        private float _repathTimer;

        public DogEatLureState(DogAI dog) : base(dog)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Friendly;

        public void SetTarget(FoodLure lure) => _lure = lure;

        public override void Enter()
        {
            Dog.Carryable.AllowPickup = true;
            _isEating = false;
            _repathTimer = 0f;
        }

        public override void Tick(float deltaTime)
        {
            if (_lure == null)
            {
                Dog.ChangeState(Dog.IdleState);
                return;
            }

            if (_isEating)
            {
                _eatTimer -= deltaTime;
                if (_eatTimer > 0f)
                {
                    return;
                }

                _lure.Consume();
                _lure = null;
                Dog.ChangeState(Dog.CalmState);
                return;
            }

            if (Dog.FlatDistanceTo(_lure.transform.position) <= Dog.Config.EatReachDistance)
            {
                _isEating = true;
                _eatTimer = Dog.Config.EatDuration;
                Dog.Stop();
                return;
            }

            // Đồ ăn có thể còn đang lăn nên cập nhật đích đến định kỳ.
            _repathTimer -= deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = RepathInterval;
                Dog.MoveTo(_lure.transform.position, Dog.Config.RunSpeed);
            }
        }

        public override void Exit()
        {
            // Bị bế đi giữa chừng thì trả lại đồ ăn cho lần sau.
            if (_lure != null)
            {
                _lure.ReleaseClaim();
                _lure = null;
            }
        }
    }

    /// <summary>Hiền sau khi được cho ăn: không sủa, cho bế. Hết thời gian thì quay lại cảnh giác.</summary>
    internal sealed class DogCalmState : DogState
    {
        private float _timer;

        public DogCalmState(DogAI dog) : base(dog)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Friendly;

        public override void Enter()
        {
            Dog.Carryable.AllowPickup = true;
            Dog.Stop();
            _timer = Dog.Config.CalmDuration;
        }

        public override void Tick(float deltaTime)
        {
            if (TryGoForLure())
            {
                return;
            }

            _timer -= deltaTime;
            if (_timer <= 0f)
            {
                Dog.ChangeState(Dog.IdleState);
            }
        }
    }

    /// <summary>Đang bị bế: tắt NavMeshAgent. Bị thả xuống thì vẫn hiền.</summary>
    internal sealed class DogCarriedState : DogState
    {
        public DogCarriedState(DogAI dog) : base(dog)
        {
        }

        public override AwarenessLevel Awareness => AwarenessLevel.Friendly;

        public override void Enter()
        {
            Dog.Carryable.AllowPickup = false;
            Dog.SetAgentActive(false);
        }

        public override void OnDropped()
        {
            Dog.SetAgentActive(true);
            Dog.ChangeState(Dog.CalmState);
        }
    }
}
