using UnityEngine;

namespace DogHeist.Gameplay.Interaction
{
    /// <summary>
    /// Thứ người chơi có thể tương tác bằng phím Interact (bế chó, mở cổng, nhặt đồ...).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Chữ hiện trên HUD, ví dụ "Bế chó".</summary>
        string Prompt { get; }

        bool CanInteract(GameObject interactor);

        void Interact(GameObject interactor);
    }
}
