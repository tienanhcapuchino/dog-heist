namespace DogHeist.Core.FSM
{
    /// <summary>
    /// Một trạng thái trong máy trạng thái. Dùng chung cho AI (chó, chủ nhà) và sau này cho luồng trận đấu.
    /// </summary>
    public interface IState
    {
        void Enter();

        void Tick(float deltaTime);

        void Exit();
    }
}
