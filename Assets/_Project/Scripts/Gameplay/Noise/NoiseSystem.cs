using System;
using UnityEngine;

namespace DogHeist.Gameplay.Noise
{
    /// <summary>
    /// Kênh phát tiếng động toàn cục. Người phát gọi Emit, người nghe (HearingSensor) đăng ký NoiseEmitted.
    /// Khi lên multiplayer, chỉ server phát và xử lý tiếng động.
    /// </summary>
    public static class NoiseSystem
    {
        public static event Action<NoiseEvent> NoiseEmitted;

        public static void Emit(NoiseEvent noise)
        {
            if (noise.Radius <= 0f)
            {
                return;
            }

            NoiseEmitted?.Invoke(noise);
        }

        // Xóa đăng ký cũ khi vào Play Mode, cần thiết nếu tắt "Domain Reload" trong Enter Play Mode Settings.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => NoiseEmitted = null;
    }
}
