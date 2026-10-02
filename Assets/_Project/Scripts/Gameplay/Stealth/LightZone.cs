using DogHeist.Gameplay.Thief;

namespace DogHeist.Gameplay.Stealth
{
    /// <summary>
    /// Vùng được chiếu sáng (đèn hiên, đèn cảm biến). Đặt trigger khớp với vùng ánh sáng của Light.
    /// </summary>
    public sealed class LightZone : StealthZone
    {
        protected override void OnThiefEntered(ThiefVisibility thief) => thief.EnterLight();

        protected override void OnThiefExited(ThiefVisibility thief) => thief.ExitLight();
    }
}
