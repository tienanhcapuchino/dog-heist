using System.Collections.Generic;
using DogHeist.Gameplay.Thief;
using UnityEngine;

namespace DogHeist.Gameplay.Stealth
{
    /// <summary>
    /// Vùng trigger ảnh hưởng tới độ lộ diện của trộm (vùng sáng, bụi cây...).
    /// Tự xử lý trường hợp vùng bị tắt khi trộm còn đứng bên trong.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class StealthZone : MonoBehaviour
    {
        private readonly HashSet<ThiefVisibility> _occupants = new();

        protected abstract void OnThiefEntered(ThiefVisibility thief);

        protected abstract void OnThiefExited(ThiefVisibility thief);

        protected virtual void Reset() => GetComponent<Collider>().isTrigger = true;

        protected virtual void OnTriggerEnter(Collider other)
        {
            var thief = other.GetComponentInParent<ThiefVisibility>();
            if (thief != null && _occupants.Add(thief))
            {
                OnThiefEntered(thief);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var thief = other.GetComponentInParent<ThiefVisibility>();
            if (thief != null && _occupants.Remove(thief))
            {
                OnThiefExited(thief);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var thief in _occupants)
            {
                if (thief != null)
                {
                    OnThiefExited(thief);
                }
            }

            _occupants.Clear();
        }
    }
}
