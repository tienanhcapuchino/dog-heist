using UnityEngine;

namespace DogHeist.Gameplay.Controls
{
    /// <summary>
    /// Nguồn điều khiển của một nhân vật. Code nhân vật chỉ đọc qua interface này,
    /// nên có thể thay bàn phím bằng input mạng (multiplayer) hoặc AI mà không sửa nhân vật.
    /// </summary>
    public interface ICharacterInput
    {
        Vector2 Move { get; }

        bool IsSprinting { get; }

        bool IsCrouching { get; }

        bool InteractPressedThisFrame { get; }

        bool ThrowLurePressedThisFrame { get; }
    }
}
