using DogHeist.Gameplay.Thief;

namespace DogHeist.Gameplay.Stealth
{
    /// <summary>
    /// Chỗ nấp (bụi cây, sau thùng phuy). Trộm phải lom khom trong vùng này và không bị chiếu sáng thì mới được tính là ẩn nấp.
    /// </summary>
    public sealed class HidingSpot : StealthZone
    {
        protected override void OnThiefEntered(ThiefVisibility thief) => thief.EnterHidingSpot();

        protected override void OnThiefExited(ThiefVisibility thief) => thief.ExitHidingSpot();
    }
}
