using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Một đoạn hàng rào hình hộp: tâm và kích thước tính theo mét.</summary>
    internal readonly struct FenceSegment
    {
        public FenceSegment(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        public Vector3 Center { get; }

        public Vector3 Size { get; }

        public bool ContainsXZ(Vector3 point) => GreyboxLayout.IsInsideBoxXZ(point, Center, Size);
    }
}
