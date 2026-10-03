using DogHeist.Gameplay.Awareness;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>Giọng của AI khi mức cảnh giác đổi.</summary>
    public enum AwarenessVoice
    {
        Owner = 0,
        Dog = 1,
    }

    /// <summary>
    /// Luật chọn tiếng, tách khỏi component để test được. Trả null nghĩa là không phát gì.
    /// </summary>
    public static class SoundRules
    {
        /// <summary>Lom khom ưu tiên trước chạy (giữ Ctrl + Shift vẫn là bước nhẹ); còn lại là đi bộ.</summary>
        public static AudioCue Footstep(SoundLibrary library, bool crouching, bool sprinting)
        {
            if (library == null)
            {
                return null;
            }

            if (crouching)
            {
                return library.FootstepCrouch;
            }

            return sprinting ? library.FootstepSprint : library.FootstepWalk;
        }

        /// <summary>
        /// Chủ nhà: tỉnh dậy nghi ngờ thì "Hửm?", phát hiện thì "Trộm!". Mất dấu hay quay lại ngủ thì im.
        /// Chó: chuyển sang thân thiện thì phát tiếng vui. Chó sủa đã có tiếng riêng từ NoiseSoundPlayer
        /// nên sang Alerted không phát thêm, tránh kêu hai lần.
        /// </summary>
        public static AudioCue ForAwarenessChange(SoundLibrary library, AwarenessVoice voice, AwarenessLevel previous, AwarenessLevel next)
        {
            if (library == null || previous == next)
            {
                return null;
            }

            switch (voice)
            {
                case AwarenessVoice.Owner:
                    if (next == AwarenessLevel.Alerted)
                    {
                        return library.OwnerShout;
                    }

                    return previous == AwarenessLevel.Sleeping && next == AwarenessLevel.Suspicious ? library.OwnerHuh : null;

                case AwarenessVoice.Dog:
                    return next == AwarenessLevel.Friendly ? library.DogHappy : null;

                default:
                    return null;
            }
        }
    }
}
