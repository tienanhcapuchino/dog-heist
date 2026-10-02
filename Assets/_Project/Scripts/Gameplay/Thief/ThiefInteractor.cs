using DogHeist.Gameplay.Controls;
using DogHeist.Gameplay.Dog;
using DogHeist.Gameplay.Interaction;
using DogHeist.Gameplay.Items;
using UnityEngine;

namespace DogHeist.Gameplay.Thief
{
    /// <summary>
    /// Các hành động của trộm ngoài di chuyển: tìm vật tương tác gần nhất, bế/thả chó, ném đồ ăn.
    /// </summary>
    [RequireComponent(typeof(ThiefMotor))]
    public sealed class ThiefInteractor : MonoBehaviour
    {
        private const int OverlapBufferSize = 16;
        private const float DropDistance = 0.8f;

        [SerializeField] private ThiefMotor _motor;

        [Tooltip("Vị trí con chó khi bị bế, thường đặt trước ngực.")]
        [SerializeField] private Transform _carryAnchor;

        [Tooltip("Vị trí đồ ăn xuất hiện khi ném, đặt phía trước nhân vật.")]
        [SerializeField] private Transform _throwOrigin;

        [SerializeField] private FoodLure _lurePrefab;

        [SerializeField] private LayerMask _interactableLayers = ~0;

        private readonly Collider[] _overlapBuffer = new Collider[OverlapBufferSize];
        private ICharacterInput _input;

        public CarryableDog CarriedDog { get; private set; }

        public bool IsCarrying => CarriedDog != null;

        public IInteractable FocusedInteractable { get; private set; }

        public int LuresRemaining { get; private set; }

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponent<ThiefMotor>();
            }

            _input = GetComponent<ICharacterInput>();

            if (_motor.Config == null || _input == null)
            {
                Debug.LogError($"{name}: ThiefInteractor cần ThiefMotor có config và một ICharacterInput.", this);
                enabled = false;
                return;
            }

            LuresRemaining = _motor.Config.StartingLures;
        }

        private void Update()
        {
            if (!_motor.InputEnabled)
            {
                FocusedInteractable = null;
                return;
            }

            FocusedInteractable = IsCarrying ? null : FindBestInteractable();

            if (_input.InteractPressedThisFrame)
            {
                if (IsCarrying)
                {
                    DropCarriedDog();
                }
                else
                {
                    FocusedInteractable?.Interact(gameObject);
                }
            }

            if (_input.ThrowLurePressedThisFrame)
            {
                TryThrowLure();
            }
        }

        public void DropCarriedDog()
        {
            if (!IsCarrying)
            {
                return;
            }

            var dog = CarriedDog;
            CarriedDog = null;
            _motor.IsCarrying = false;
            dog.Detach(transform.position + transform.forward * DropDistance);
        }

        internal void BeginCarry(CarryableDog dog)
        {
            if (dog == null || IsCarrying)
            {
                return;
            }

            CarriedDog = dog;
            _motor.IsCarrying = true;
            dog.AttachTo(_carryAnchor != null ? _carryAnchor : transform);
        }

        private IInteractable FindBestInteractable()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _motor.Config.InteractRadius,
                _overlapBuffer,
                _interactableLayers,
                QueryTriggerInteraction.Collide);

            IInteractable best = null;
            var bestSqrDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = _overlapBuffer[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract(gameObject))
                {
                    continue;
                }

                var sqrDistance = (_overlapBuffer[i].transform.position - transform.position).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                best = candidate;
                bestSqrDistance = sqrDistance;
            }

            return best;
        }

        private void TryThrowLure()
        {
            if (_lurePrefab == null || LuresRemaining <= 0 || IsCarrying)
            {
                return;
            }

            var origin = _throwOrigin != null ? _throwOrigin : transform;
            var lure = Instantiate(_lurePrefab, origin.position, origin.rotation);

            if (lure.TryGetComponent(out Rigidbody body))
            {
                var throwDirection = (transform.forward + Vector3.up * _motor.Config.LureThrowArc).normalized;
                body.AddForce(throwDirection * _motor.Config.LureThrowForce, ForceMode.VelocityChange);
            }

            LuresRemaining--;
        }
    }
}
