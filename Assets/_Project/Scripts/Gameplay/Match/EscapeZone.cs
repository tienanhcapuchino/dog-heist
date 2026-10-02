using DogHeist.Gameplay.Thief;
using UnityEngine;

namespace DogHeist.Gameplay.Match
{
    /// <summary>
    /// Vùng thoát ngoài cổng. Trộm bước vào khi đang bế chó thì thắng ván.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class EscapeZone : MonoBehaviour
    {
        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other) => TryEscape(other);

        private void OnTriggerStay(Collider other) => TryEscape(other);

        private static void TryEscape(Collider other)
        {
            var thief = other.GetComponentInParent<ThiefInteractor>();
            if (thief != null && thief.IsCarrying)
            {
                MatchEvents.RaiseThiefEscaped();
            }
        }
    }
}
