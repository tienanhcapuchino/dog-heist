using UnityEngine;

namespace DogHeist.Gameplay.Thief
{
    /// <summary>
    /// Thông số của trộm. Tạo asset qua menu Create > DogHeist > Configs > Thief Config,
    /// chỉnh số ở đây thay vì sửa code để cân bằng game.
    /// </summary>
    [CreateAssetMenu(fileName = "ThiefConfig", menuName = "DogHeist/Configs/Thief Config")]
    public sealed class ThiefConfig : ScriptableObject
    {
        [field: Header("Di chuyển (m/s)")]
        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; } = 3f;

        [field: SerializeField, Min(0f)] public float CrouchSpeed { get; private set; } = 1.5f;

        [field: SerializeField, Min(0f)] public float SprintSpeed { get; private set; } = 5.5f;

        [field: Tooltip("Hệ số tốc độ khi đang bế chó.")]
        [field: SerializeField, Range(0.1f, 1f)] public float CarrySpeedMultiplier { get; private set; } = 0.7f;

        [field: Tooltip("Tốc độ xoay người (độ/giây).")]
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; private set; } = 720f;

        [field: SerializeField] public float Gravity { get; private set; } = -20f;

        [field: Header("Tiếng ồn (bán kính, mét)")]
        [field: SerializeField, Min(0f)] public float CrouchNoiseRadius { get; private set; } = 1.5f;

        [field: SerializeField, Min(0f)] public float WalkNoiseRadius { get; private set; } = 4f;

        [field: SerializeField, Min(0f)] public float SprintNoiseRadius { get; private set; } = 9f;

        [field: Tooltip("Tiếng ồn cộng thêm khi đang bế chó.")]
        [field: SerializeField, Min(0f)] public float CarryNoiseBonus { get; private set; } = 2f;

        [field: Tooltip("Khoảng thời gian giữa hai lần phát tiếng bước chân (giây).")]
        [field: SerializeField, Min(0.05f)] public float NoiseInterval { get; private set; } = 0.4f;

        [field: Header("Độ lộ diện (0 = vô hình, 1 = lộ hoàn toàn)")]
        [field: SerializeField, Range(0f, 1f)] public float DarkVisibility { get; private set; } = 0.45f;

        [field: SerializeField, Range(0f, 1f)] public float LitVisibility { get; private set; } = 1f;

        [field: SerializeField, Range(0f, 1f)] public float CrouchVisibilityMultiplier { get; private set; } = 0.6f;

        [field: SerializeField, Range(0f, 1f)] public float HiddenVisibility { get; private set; } = 0.1f;

        [field: Header("Tương tác và đồ nghề")]
        [field: SerializeField, Min(0.1f)] public float InteractRadius { get; private set; } = 1.5f;

        [field: SerializeField, Min(0)] public int StartingLures { get; private set; } = 2;

        [field: SerializeField, Min(0f)] public float LureThrowForce { get; private set; } = 6f;

        [field: Tooltip("Độ cong khi ném: 0 là ném thẳng, 1 là ném chếch 45 độ.")]
        [field: SerializeField, Range(0f, 1f)] public float LureThrowArc { get; private set; } = 0.5f;

        public float MaxNoiseRadius => SprintNoiseRadius + CarryNoiseBonus;
    }
}
