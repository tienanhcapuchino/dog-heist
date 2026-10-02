using Unity.AI.Navigation;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các điểm mốc trong môi trường mà nhân vật AI cần tham chiếu.</summary>
    internal sealed class GreyboxEnvironment
    {
        public Transform DogHome { get; set; }

        public Transform OwnerBed { get; set; }

        public Transform[] PatrolWaypoints { get; set; }

        public NavMeshSurface NavMesh { get; set; }
    }
}
