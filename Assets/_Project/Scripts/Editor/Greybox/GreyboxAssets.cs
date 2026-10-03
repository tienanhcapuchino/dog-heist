using System;
using System.Collections.Generic;
using System.IO;
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Audio;
using DogHeist.Gameplay.Items;
using DogHeist.Gameplay.Thief;
using DogHeist.UI.Awareness;
using TMPro;
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
        private const string VietnameseFontFileName = "BeVietnamPro-Regular.ttf";
        private const string VietnameseFontAssetName = "BeVietnamPro SDF.asset";

        // Tên trùng tên property của SoundLibrary; mỗi cue là một file .asset riêng để người dùng kéo clip vào.
        private static readonly string[] s_cueNames =
        {
            nameof(SoundLibrary.FootstepCrouch),
            nameof(SoundLibrary.FootstepWalk),
            nameof(SoundLibrary.FootstepSprint),
            nameof(SoundLibrary.DogBark),
            nameof(SoundLibrary.DogHappy),
            nameof(SoundLibrary.OwnerHuh),
            nameof(SoundLibrary.OwnerShout),
            nameof(SoundLibrary.LureLand),
            nameof(SoundLibrary.StingerWin),
            nameof(SoundLibrary.StingerLose),
        };

        // Nhạc kết thúc ván nghe đều mọi nơi (2D), không nhỏ dần theo khoảng cách.
        private static readonly HashSet<string> s_cues2D = new HashSet<string>
        {
            nameof(SoundLibrary.StingerWin),
            nameof(SoundLibrary.StingerLose),
        };

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

        public SoundLibrary Sounds { get; private set; }

        public AwarenessIndicatorStyle IndicatorStyle { get; private set; }

        /// <summary>Font tiếng Việt cho HUD và dấu trên đầu AI; null khi người dùng chưa tải file font.</summary>
        public TMP_FontAsset UiFont { get; private set; }

        public static GreyboxAssets LoadOrCreate(string assetRoot)
        {
            var configFolder = $"{assetRoot}/Settings/Configs";
            var materialFolder = $"{assetRoot}/Art/Materials/Greybox";
            var itemFolder = $"{assetRoot}/Prefabs/Items";
            var cueFolder = $"{assetRoot}/Audio/Cues";
            EnsureFolder(configFolder);
            EnsureFolder(materialFolder);
            EnsureFolder(itemFolder);
            EnsureFolder(cueFolder);

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
                LureMaterial = LoadOrCreateMaterial($"{materialFolder}/Lure.mat", new Color(0.60f, 0.35f, 0.15f)),
                Sounds = LoadOrCreateSoundLibrary($"{assetRoot}/Audio/SoundLibrary.asset", cueFolder),
                IndicatorStyle = LoadOrCreateConfig<AwarenessIndicatorStyle>($"{assetRoot}/Settings/AwarenessIndicatorStyle.asset"),
                UiFont = LoadOrCreateVietnameseFont($"{assetRoot}/Art/Fonts")
            };

            var lurePrefab = LoadOrCreateLurePrefab($"{itemFolder}/FoodLure.prefab", assets.LureMaterial);
            assets.LurePrefab = EnsureLureImpactSound(lurePrefab, assets.Sounds);
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

        /// <summary>
        /// Thêm tiếng "bịch" cho prefab đồ ăn, cả prefab mới lẫn prefab tạo từ trước khi có âm thanh.
        /// Prefab đã có ImpactSound trỏ đúng thư viện thì không đụng tới.
        /// </summary>
        private static FoodLure EnsureLureImpactSound(FoodLure lure, SoundLibrary sounds)
        {
            var existing = lure.GetComponent<ImpactSound>();
            if (existing != null && new SerializedObject(existing).FindProperty("_library").objectReferenceValue != null)
            {
                return lure;
            }

            var path = AssetDatabase.GetAssetPath(lure);
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var audioSource = contents.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = contents.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1f;
                }

                var impact = contents.GetComponent<ImpactSound>();
                if (impact == null)
                {
                    impact = contents.AddComponent<ImpactSound>();
                }

                SerializedWiring.Assign(impact, "_library", sounds);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            return AssetDatabase.LoadAssetAtPath<FoodLure>(path);
        }

        private static SoundLibrary LoadOrCreateSoundLibrary(string path, string cueFolder)
        {
            var library = LoadOrCreateConfig<SoundLibrary>(path);
            var serialized = new SerializedObject(library);
            var changed = false;

            foreach (var cueName in s_cueNames)
            {
                var cue = LoadOrCreateCue($"{cueFolder}/{cueName}.asset", s_cues2D.Contains(cueName));

                // Chỉ điền ô đang trống: ô người dùng đã đổi sang cue khác thì giữ nguyên.
                if (serialized.FindProperty($"<{cueName}>k__BackingField").objectReferenceValue == null)
                {
                    library.Assign(cueName, cue);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(library);
            }

            return library;
        }

        private static AudioCue LoadOrCreateCue(string path, bool is2D)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioCue>(path);
            if (existing != null)
            {
                return existing;
            }

            var cue = ScriptableObject.CreateInstance<AudioCue>();
            if (is2D)
            {
                var serialized = new SerializedObject(cue);
                serialized.FindProperty("_spatialBlend").floatValue = 0f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetDatabase.CreateAsset(cue, path);
            return cue;
        }

        /// <summary>
        /// Tạo TMP font asset dạng động (Dynamic) từ file Be Vietnam Pro: ký tự được thêm vào atlas khi cần,
        /// nên đủ mọi dấu tiếng Việt mà không phải chọn trước bảng ký tự.
        /// LiberationSans làm font dự phòng cho ký tự Be Vietnam Pro không có (ví dụ ♥).
        /// </summary>
        private static TMP_FontAsset LoadOrCreateVietnameseFont(string fontFolder)
        {
            var assetPath = $"{fontFolder}/{VietnameseFontAssetName}";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var fontPath = $"{fontFolder}/{VietnameseFontFileName}";
            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogWarning(
                    $"[Greybox] Chưa có font tiếng Việt: tải Be Vietnam Pro (Google Fonts) và đặt file vào {fontPath}, rồi chạy lại menu. " +
                    "Tạm dùng font mặc định của TextMeshPro.");
                return null;
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(font);
            if (fontAsset == null)
            {
                Debug.LogError($"[Greybox] Không tạo được font asset từ {fontPath}.");
                return null;
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(VietnameseFontAssetName);
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            // Atlas và material phải lưu chung file với font asset, nếu không sẽ mất khi đóng Unity.
            foreach (var atlas in fontAsset.atlasTextures)
            {
                if (atlas != null)
                {
                    atlas.name = $"{fontAsset.name} Atlas";
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }

            fontAsset.material.name = $"{fontAsset.name} Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            var fallback = TMP_Settings.defaultFontAsset;
            if (fallback != null)
            {
                fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
            }
            else
            {
                Debug.LogWarning("[Greybox] Không tìm thấy font mặc định của TextMeshPro để làm font dự phòng; ký tự ♥ có thể không hiện.");
            }

            EditorUtility.SetDirty(fontAsset);
            return fontAsset;
        }
    }
}
