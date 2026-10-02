using System.Collections.Generic;
using UnityEngine;

namespace DogHeist.Gameplay.Items
{
    /// <summary>
    /// Đồ ăn trộm ném ra để dụ chó. Chó sẽ chạy tới ăn và trở nên hiền, cho phép bế đi.
    /// Mỗi miếng chỉ một con chó được "nhận" để tránh hai con tranh nhau.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FoodLure : MonoBehaviour
    {
        private static readonly List<FoodLure> s_active = new();

        [field: SerializeField, Min(0f)] public float AttractRadius { get; private set; } = 8f;

        [field: SerializeField, Min(0f)] public float EatDuration { get; private set; } = 4f;

        public bool IsClaimed { get; private set; }

        public static FoodLure FindNearestUnclaimed(Vector3 position)
        {
            FoodLure nearest = null;
            var nearestSqrDistance = float.MaxValue;

            foreach (var lure in s_active)
            {
                if (lure.IsClaimed)
                {
                    continue;
                }

                var sqrDistance = (lure.transform.position - position).sqrMagnitude;
                var radius = lure.AttractRadius;
                if (sqrDistance > radius * radius || sqrDistance >= nearestSqrDistance)
                {
                    continue;
                }

                nearest = lure;
                nearestSqrDistance = sqrDistance;
            }

            return nearest;
        }

        public bool TryClaim()
        {
            if (IsClaimed)
            {
                return false;
            }

            IsClaimed = true;
            return true;
        }

        public void ReleaseClaim() => IsClaimed = false;

        public void Consume()
        {
            IsClaimed = true;
            Destroy(gameObject);
        }

        private void OnEnable() => s_active.Add(this);

        private void OnDisable() => s_active.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => s_active.Clear();
    }
}
