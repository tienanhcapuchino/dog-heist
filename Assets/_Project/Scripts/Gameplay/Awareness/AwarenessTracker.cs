using System;

namespace DogHeist.Gameplay.Awareness
{
    /// <summary>
    /// Giữ mức cảnh giác hiện tại và chỉ báo khi mức đổi
    /// (ví dụ chủ nhà từ Investigate sang Patrol vẫn là Suspicious nên không báo lại).
    /// Lớp thuần, không cần scene nên test được bằng EditMode test.
    /// </summary>
    public sealed class AwarenessTracker
    {
        public event Action<AwarenessLevel> Changed;

        public AwarenessLevel Current { get; private set; } = AwarenessLevel.None;

        public void Set(AwarenessLevel level)
        {
            if (level == Current)
            {
                return;
            }

            Current = level;
            Changed?.Invoke(level);
        }
    }
}
