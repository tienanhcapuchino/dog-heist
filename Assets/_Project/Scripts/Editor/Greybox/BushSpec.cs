using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Vị trí (trên mặt đất) và đường kính của một bụi cây.</summary>
    internal readonly struct BushSpec
    {
        public BushSpec(Vector3 position, float diameter)
        {
            Position = position;
            Diameter = diameter;
        }

        public Vector3 Position { get; }

        public float Diameter { get; }
    }
}
