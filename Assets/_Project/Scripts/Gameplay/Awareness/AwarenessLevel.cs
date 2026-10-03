namespace DogHeist.Gameplay.Awareness
{
    /// <summary>
    /// Mức cảnh giác của một AI, dùng để hiện dấu trên đầu và chọn tiếng.
    /// UI và âm thanh chỉ đọc mức này, không cần biết AI có những trạng thái nào.
    /// </summary>
    public enum AwarenessLevel
    {
        /// <summary>Bình thường, không hiện dấu.</summary>
        None = 0,

        /// <summary>Đang ngủ (Zzz).</summary>
        Sleeping = 1,

        /// <summary>Nghi ngờ, đi xem hoặc đi tuần (?).</summary>
        Suspicious = 2,

        /// <summary>Đã phát hiện: chủ nhà đuổi, chó sủa (!).</summary>
        Alerted = 3,

        /// <summary>Thân thiện: chó đang ăn, hiền hoặc được bế (♥).</summary>
        Friendly = 4,
    }
}
