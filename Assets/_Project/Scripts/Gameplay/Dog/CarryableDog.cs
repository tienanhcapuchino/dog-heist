using System;
using DogHeist.Gameplay.Interaction;
using DogHeist.Gameplay.Thief;
using UnityEngine;

namespace DogHeist.Gameplay.Dog
{
    /// <summary>
    /// Phần "vật có thể bế" của con chó. Không chứa AI: DogAI (assembly AI) quyết định
    /// khi nào chó cho bế bằng cách đặt AllowPickup, và nghe sự kiện PickedUp/Dropped.
    /// </summary>
    public sealed class CarryableDog : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _pickupPrompt = "Bế chó";

        private Collider[] _colliders = Array.Empty<Collider>();

        public event Action PickedUp;

        public event Action Dropped;

        public bool AllowPickup { get; set; }

        public bool IsCarried { get; private set; }

        public string Prompt => _pickupPrompt;

        private void Awake() => _colliders = GetComponentsInChildren<Collider>();

        public bool CanInteract(GameObject interactor) => AllowPickup && !IsCarried;

        public void Interact(GameObject interactor)
        {
            if (interactor == null || !CanInteract(interactor))
            {
                return;
            }

            if (interactor.TryGetComponent(out ThiefInteractor thief))
            {
                thief.BeginCarry(this);
            }
        }

        internal void AttachTo(Transform anchor)
        {
            IsCarried = true;

            // Báo trước để AI kịp tắt NavMeshAgent, nếu không agent sẽ giật con chó về NavMesh.
            PickedUp?.Invoke();

            SetCollidersEnabled(false);
            transform.SetParent(anchor, worldPositionStays: false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        internal void Detach(Vector3 dropPosition)
        {
            transform.SetParent(null, worldPositionStays: true);
            transform.position = dropPosition;
            SetCollidersEnabled(true);
            IsCarried = false;
            Dropped?.Invoke();
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (var dogCollider in _colliders)
            {
                dogCollider.enabled = isEnabled;
            }
        }
    }
}
