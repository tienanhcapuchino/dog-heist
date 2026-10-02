using System;

namespace DogHeist.Core.FSM
{
    /// <summary>
    /// Máy trạng thái tối giản: luôn gọi Exit của trạng thái cũ trước Enter của trạng thái mới.
    /// Không phụ thuộc Unity nên có thể test bằng EditMode test.
    /// </summary>
    public sealed class StateMachine
    {
        public event Action<IState, IState> StateChanged;

        public IState CurrentState { get; private set; }

        public void ChangeState(IState next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (ReferenceEquals(next, CurrentState))
            {
                return;
            }

            var previous = CurrentState;
            previous?.Exit();
            CurrentState = next;
            CurrentState.Enter();
            StateChanged?.Invoke(previous, next);
        }

        public void Tick(float deltaTime) => CurrentState?.Tick(deltaTime);
    }
}
