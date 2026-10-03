using UnityEngine;

namespace DogHeist.Gameplay.Awareness
{
    /// <summary>
    /// Unity không serialize được field kiểu interface, nên Inspector lưu nguồn dưới dạng MonoBehaviour.
    /// Lớp này đổi MonoBehaviour đó thành IAwarenessSource và báo lỗi rõ ràng khi kéo nhầm component.
    /// </summary>
    public static class AwarenessSourceResolver
    {
        /// <returns>Nguồn cảnh giác, hoặc null (kèm log lỗi) khi trống hoặc component không cài IAwarenessSource.</returns>
        public static IAwarenessSource Resolve(MonoBehaviour candidate, Object context)
        {
            if (candidate == null)
            {
                Debug.LogError("[Awareness] Chưa gán nguồn cảnh giác (chó hoặc chủ nhà).", context);
                return null;
            }

            if (candidate is IAwarenessSource source)
            {
                return source;
            }

            Debug.LogError($"[Awareness] {candidate.GetType().Name} không cài IAwarenessSource; hãy kéo DogAI hoặc OwnerAI vào.", context);
            return null;
        }
    }
}
