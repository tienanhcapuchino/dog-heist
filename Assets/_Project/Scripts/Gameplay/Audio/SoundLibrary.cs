using System;
using UnityEngine;

namespace DogHeist.Gameplay.Audio
{
    /// <summary>Kho tham chiếu tới mọi AudioCue của game, để người phát tiếng chỉ cần một ô kéo thả.</summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "DogHeist/Sound Library")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [field: Header("Bước chân của trộm")]
        [field: SerializeField] public AudioCue FootstepCrouch { get; private set; }

        [field: SerializeField] public AudioCue FootstepWalk { get; private set; }

        [field: SerializeField] public AudioCue FootstepSprint { get; private set; }

        [field: Header("Chó")]
        [field: SerializeField] public AudioCue DogBark { get; private set; }

        [field: SerializeField] public AudioCue DogHappy { get; private set; }

        [field: Header("Chủ nhà")]
        [field: SerializeField] public AudioCue OwnerHuh { get; private set; }

        [field: SerializeField] public AudioCue OwnerShout { get; private set; }

        [field: Header("Khác")]
        [field: SerializeField] public AudioCue LureLand { get; private set; }

        [field: SerializeField] public AudioCue StingerWin { get; private set; }

        [field: SerializeField] public AudioCue StingerLose { get; private set; }

        /// <summary>Gán cue theo tên property (dùng cho công cụ greybox).</summary>
        internal void Assign(string cueName, AudioCue cue)
        {
            switch (cueName)
            {
                case nameof(FootstepCrouch): FootstepCrouch = cue; break;
                case nameof(FootstepWalk): FootstepWalk = cue; break;
                case nameof(FootstepSprint): FootstepSprint = cue; break;
                case nameof(DogBark): DogBark = cue; break;
                case nameof(DogHappy): DogHappy = cue; break;
                case nameof(OwnerHuh): OwnerHuh = cue; break;
                case nameof(OwnerShout): OwnerShout = cue; break;
                case nameof(LureLand): LureLand = cue; break;
                case nameof(StingerWin): StingerWin = cue; break;
                case nameof(StingerLose): StingerLose = cue; break;
                default: throw new ArgumentException($"[Audio] SoundLibrary không có cue tên '{cueName}'.", nameof(cueName));
            }
        }
    }
}
