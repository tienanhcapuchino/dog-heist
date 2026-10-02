using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Các hàm tạo khối hình đơn giản cho màn greybox. Gốc [Greybox] đặt ở (0,0,0) nên tọa độ local trùng tọa độ thế giới.</summary>
    internal static class GreyboxPrimitives
    {
        public static GameObject Create(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider = true)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = localScale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                Object.DestroyImmediate(gameObject.GetComponent<Collider>());
            }

            return gameObject;
        }

        public static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            return gameObject;
        }

        public static GameObject CreateTrigger(string name, Transform parent, Vector3 center, Vector3 size)
        {
            var gameObject = CreateEmpty(name, parent, center);
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            return gameObject;
        }

        public static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }
    }
}
