using UnityEngine;

namespace DogHeist.AI.Dog
{
    [CreateAssetMenu(fileName = "DogConfig", menuName = "DogHeist/Configs/Dog Config")]
    public sealed class DogConfig : ScriptableObject
    {
        [field: Header("Di chuyển")]
        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; } = 1.5f;

        [field: SerializeField, Min(0f)] public float RunSpeed { get; private set; } = 4f;

        [field: SerializeField, Min(0f)] public float TurnSpeed { get; private set; } = 360f;

        [field: Tooltip("Bán kính chó lang thang quanh chuồng.")]
        [field: SerializeField, Min(0f)] public float WanderRadius { get; private set; } = 3f;

        [field: SerializeField, Min(0.1f)] public float WanderInterval { get; private set; } = 4f;

        [field: Header("Cảnh giác")]
        [field: Tooltip("Khoảng cách chó phát hiện trộm khi trộm lộ hoàn toàn (nhân với độ lộ diện).")]
        [field: SerializeField, Min(0f)] public float NoticeDistance { get; private set; } = 5f;

        [field: Tooltip("Độ thính tai của chó: nhân với bán kính tiếng ồn nghe được.")]
        [field: SerializeField, Min(0f)] public float HearingSensitivity { get; private set; } = 1f;

        [field: Tooltip("Thời gian chó tiếp tục sủa sau khi mất dấu trộm.")]
        [field: SerializeField, Min(0f)] public float AlertDuration { get; private set; } = 5f;

        [field: SerializeField, Min(0.1f)] public float BarkInterval { get; private set; } = 1.2f;

        [field: SerializeField, Min(0f)] public float BarkNoiseRadius { get; private set; } = 16f;

        [field: Header("Đồ ăn và trạng thái hiền")]
        [field: SerializeField, Min(0.1f)] public float EatReachDistance { get; private set; } = 0.8f;

        [field: Tooltip("Khoảng cách chó nhận ra đồ ăn dụ.")]
        [field: SerializeField, Min(0f)] public float LureNoticeDistance { get; private set; } = 8f;

        [field: Tooltip("Thời gian chó đứng ăn.")]
        [field: SerializeField, Min(0f)] public float EatDuration { get; private set; } = 4f;

        [field: Tooltip("Thời gian chó hiền (cho bế) sau khi ăn xong.")]
        [field: SerializeField, Min(0f)] public float CalmDuration { get; private set; } = 15f;
    }
}
