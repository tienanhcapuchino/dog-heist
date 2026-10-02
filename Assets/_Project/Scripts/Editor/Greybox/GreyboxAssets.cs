using System;
using System.IO;
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Items;
using DogHeist.Gameplay.Thief;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Nạp hoặc tạo các asset mà màn greybox cần. Asset đã tồn tại thì giữ nguyên,
    /// để thông số người dùng đã cân bằng trong config không bị reset khi dựng lại màn.
    /// </summary>
    internal sealed class GreyboxAssets
    {
        public const string DefaultAssetRoot = "Assets/_Project";
        public const string CharactersLayerName = "Characters";

        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string FallbackShaderName = "Standard";
        private const int FirstUserLayer = 8;
        private const int LayerCount = 32;

        public ThiefConfig ThiefConfig { get; private set; }

        public DogConfig DogConfig { get; private set; }

        public OwnerConfig OwnerConfig { get; private set; }

        public FoodLure LurePrefab { get; private set; }

        public int CharactersLayer { get; private set; }

        public Material GroundMaterial { get; private set; }

        public Material FenceMaterial { get; private set; }

        public Material BuildingMaterial { get; private set; }

        public Material BushMaterial { get; private set; }

        public Material ThiefMaterial { get; private set; }

        public Material DogMaterial { get; private set; }

        public Material OwnerMaterial { get; private set; }

        public Material LureMaterial { get; private set; }

        public static GreyboxAssets LoadOrCreate(string assetRoot)
        {
            var configFolder = $"{assetRoot}/Settings/Configs";
            var materialFolder = $"{assetRoot}/Art/Materials/Greybox";
            var itemFolder = $"{assetRoot}/Prefabs/Items";
            EnsureFolder(configFolder);
            EnsureFolder(materialFolder);
            EnsureFolder(itemFolder);

            var assets = new GreyboxAssets
            {
                CharactersLayer = EnsureLayer(CharactersLayerName),
                ThiefConfig = LoadOrCreateConfig<ThiefConfig>($"{configFolder}/ThiefConfig.asset"),
                DogConfig = LoadOrCreateConfig<DogConfig>($"{configFolder}/DogConfig.asset"),
                OwnerConfig = LoadOrCreateConfig<OwnerConfig>($"{configFolder}/OwnerConfig.asset"),
                GroundMaterial = LoadOrCreateMaterial($"{materialFolder}/Ground.mat", new Color(0.20f, 0.32f, 0.20f)),
                FenceMaterial = LoadOrCreateMaterial($"{materialFolder}/Fence.mat", new Color(0.45f, 0.35f, 0.25f)),
                BuildingMaterial = LoadOrCreateMaterial($"{materialFolder}/Building.mat", new Color(0.75f, 0.72f, 0.65f)),
                BushMaterial = LoadOrCreateMaterial($"{materialFolder}/Bush.mat", new Color(0.15f, 0.45f, 0.18f)),
                ThiefMaterial = LoadOrCreateMaterial($"{materialFolder}/Thief.mat", new Color(0.15f, 0.20f, 0.45f)),
                DogMaterial = LoadOrCreateMaterial($"{materialFolder}/Dog.mat", new Color(0.95f, 0.75f, 0.30f)),
                OwnerMaterial = LoadOrCreateMaterial($"{materialFolder}/Owner.mat", new Color(0.80f, 0.25f, 0.20f)),
                LureMaterial = LoadOrCreateMaterial($"{materialFolder}/Lure.mat", new Color(0.60f, 0.35f, 0.15f))
            };

            assets.LurePrefab = LoadOrCreateLurePrefab($"{itemFolder}/FoodLure.prefab", assets.LureMaterial);
            AssetDatabase.SaveAssets();
            return assets;
        }

        public static int EnsureLayer(string layerName)
        {
            var existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0)
            {
                return existing;
            }

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (var i = FirstUserLayer; i < LayerCount; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(layer.stringValue))
                {
                    continue;
                }

                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }

            throw new InvalidOperationException(
                $"[Greybox] Không còn chỗ trống cho layer '{layerName}' (layer 8–31 đã dùng hết). Xóa bớt layer trong Tags and Layers.");
        }

        public static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
            {
                throw new ArgumentException($"[Greybox] Đường dẫn thư mục không hợp lệ: {folderPath}", nameof(folderPath));
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        private static T LoadOrCreateConfig<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static Material LoadOrCreateMaterial(string path, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogWarning("[Greybox] Không tìm thấy shader URP Lit, dùng shader Standard. Project có tạo từ template Universal 3D không?");
                shader = Shader.Find(FallbackShaderName);
            }

            var material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static FoodLure LoadOrCreateLurePrefab(string path, Material material)
        {
            var existing = AssetDatabase.LoadAssetAtPath<FoodLure>(path);
            if (existing != null)
            {
                return existing;
            }

            var temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            try
            {
                temporary.name = "FoodLure";
                temporary.transform.localScale = Vector3.one * 0.3f;
                temporary.GetComponent<Renderer>().sharedMaterial = material;
                var body = temporary.AddComponent<Rigidbody>();
                body.mass = 0.2f;
                body.collisionDetectionMode = CollisionDetectionMode.Continuous;
                temporary.AddComponent<FoodLure>();

                var prefab = PrefabUtility.SaveAsPrefabAsset(temporary, path);
                return prefab.GetComponent<FoodLure>();
            }
            finally
            {
                Object.DestroyImmediate(temporary);
            }
        }
    }
}
