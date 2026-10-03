using System;
using UnityEngine;

namespace DogHeist.Gameplay.Awareness
{
    /// <summary>
    /// Nguồn mức cảnh giác (chó, chủ nhà). Đặt ở Gameplay để UI đọc được mà không tham chiếu AI.
    /// </summary>
    public interface IAwarenessSource
    {
        AwarenessLevel Awareness { get; }

        /// <summary>Chỉ phát khi mức thật sự đổi.</summary>
        event Action<AwarenessLevel> AwarenessChanged;

        /// <summary>Điểm trên đầu để đặt dấu.</summary>
        Transform IndicatorAnchor { get; }
    }
}
