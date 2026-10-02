using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using Unity.AI.Navigation;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các object chính vừa dựng, để menu và test dùng tiếp.</summary>
    internal sealed class GreyboxBuildResult
    {
        public GameObject Root { get; set; }

        public ThiefMotor Thief { get; set; }

        public DogAI Dog { get; set; }

        public OwnerAI Owner { get; set; }

        public MatchManager Match { get; set; }

        public NavMeshSurface NavMesh { get; set; }
    }
}
