using UnityEngine;

namespace DogHeist.AI.Owner
{
    [CreateAssetMenu(fileName = "OwnerConfig", menuName = "DogHeist/Configs/Owner Config")]
    public sealed class OwnerConfig : ScriptableObject
    {
        [field: Header("Di chuyển (m/s)")]
        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; } = 2f;

        [field: SerializeField, Min(0f)] public float InvestigateSpeed { get; private set; } = 2.5f;

        [field: SerializeField, Min(0f)] public float RunSpeed { get; private set; } = 4.5f;

        [field: SerializeField, Min(0f)] public float LookAroundSpeed { get; private set; } = 90f;

        [field: Header("Thính giác")]
        [field: Tooltip("Đang ngủ thì nghe kém: chỉ tiếng chó sủa hoặc tiếng chạy gần mới đánh thức được.")]
        [field: SerializeField, Min(0f)] public float SleepingHearingSensitivity { get; private set; } = 0.6f;

        [field: SerializeField, Min(0f)] public float AwakeHearingSensitivity { get; private set; } = 1f;

        [field: Header("Tầm nhìn")]
        [field: Tooltip("Tầm nhìn tối đa khi trộm lộ hoàn toàn (nhân với độ lộ diện).")]
        [field: SerializeField, Min(0f)] public float VisionRange { get; private set; } = 12f;

        [field: Tooltip("Góc nhìn hình nón, tính bằng độ.")]
        [field: SerializeField, Range(1f, 360f)] public float VisionFieldOfView { get; private set; } = 110f;

        [field: Header("Hành vi")]
        [field: Tooltip("Thời gian đứng nhìn quanh khi tới chỗ có tiếng động.")]
        [field: SerializeField, Min(0f)] public float InvestigateLookTime { get; private set; } = 4f;

        [field: Tooltip("Thời gian đi tuần trước khi quay lại ngủ.")]
        [field: SerializeField, Min(0f)] public float PatrolDuration { get; private set; } = 25f;

        [field: SerializeField, Min(0f)] public float WaypointWaitTime { get; private set; } = 1.5f;

        [field: Tooltip("Mất dấu trộm quá thời gian này thì chuyển sang tìm kiếm.")]
        [field: SerializeField, Min(0f)] public float LoseSightTime { get; private set; } = 3f;

        [field: SerializeField, Min(0.05f)] public float ChaseRepathInterval { get; private set; } = 0.2f;

        [field: SerializeField, Min(0.1f)] public float CatchDistance { get; private set; } = 1.2f;
    }
}
