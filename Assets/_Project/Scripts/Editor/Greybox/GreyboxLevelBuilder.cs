using System.Collections.Generic;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Dựng màn greybox Level01 thay cho 18 bước làm tay trong README.
    /// Mọi thứ sinh ra nằm dưới một gốc [Greybox]; dựng lại chỉ thay gốc này, giữ nguyên vật người dùng tự thêm.
    /// </summary>
    internal static class GreyboxLevelBuilder
    {
        public const string RootName = "[Greybox]";
        public const string ScenePath = "Assets/_Project/Scenes/Level01_Neighborhood.unity";

        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string NavMeshDataPath = "Assets/_Project/Scenes/Level01_Neighborhood_NavMesh.asset";
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string TmpSettingsResource = "TMP Settings";
        private static readonly Color NightAmbient = new Color(0.08f, 0.10f, 0.16f);

        [MenuItem("DogHeist/Tools/Build Greybox Level")]
        private static void BuildFromMenu()
        {
            var missing = FindMissingPrerequisites();
            if (missing.Count > 0)
            {
                Debug.LogError("[Greybox] Chưa dựng được màn vì project còn thiếu:\n- " + string.Join("\n- ", missing));
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = OpenOrCreateScene();
            SceneManager.SetActiveScene(scene);
            var result = BuildInto(scene, new GreyboxBuildOptions());
            ApplyNightAmbient();
            BakeNavMesh(result.NavMesh);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            GreyboxBuildSettings.EnsureSceneInBuild(ScenePath);
            Selection.activeGameObject = result.Thief.gameObject;
            Debug.Log($"[Greybox] Đã dựng xong {ScenePath}. Bấm Play để chơi thử.");
        }

        public static List<string> FindMissingPrerequisites()
        {
            var missing = new List<string>();
            if (Resources.Load<TMP_Settings>(TmpSettingsResource) == null)
            {
                missing.Add("TMP Essential Resources (Window > TextMeshPro > Import TMP Essential Resources).");
            }

            if (GraphicsSettings.defaultRenderPipeline == null || Shader.Find(LitShaderName) == null)
            {
                missing.Add("Universal Render Pipeline: project cần tạo từ template Universal 3D (xem Task 0 của kế hoạch).");
            }

            return missing;
        }

        public static GreyboxBuildResult BuildInto(Scene scene, GreyboxBuildOptions options)
        {
            RemoveExistingRoot(scene);
            var assets = GreyboxAssets.LoadOrCreate(options.AssetRoot);

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            var environment = GreyboxEnvironmentFactory.Build(root.transform, assets);
            var characters = GreyboxPrimitives.CreateEmpty("Characters", root.transform, Vector3.zero).transform;
            var thief = GreyboxCharacterFactory.CreateThief(characters, assets);
            var visibility = thief.GetComponent<ThiefVisibility>();
            var dog = GreyboxCharacterFactory.CreateDog(characters, assets, visibility, environment.DogHome);
            var owner = GreyboxCharacterFactory.CreateOwner(characters, assets, visibility,
                environment.OwnerBed, environment.PatrolWaypoints);

            var match = GreyboxPrimitives.CreateEmpty("Match", root.transform, Vector3.zero).AddComponent<MatchManager>();
            SerializedWiring.Assign(match, "_thief", thief);

            var camera = GreyboxCameraFactory.Create(root.transform, thief.transform);
            SerializedWiring.Assign(thief, "_cameraTransform", camera.transform);
            var hud = GreyboxHudFactory.Create(root.transform, thief, match);

            environment.NavMesh.layerMask = ~(1 << assets.CharactersLayer);

            return new GreyboxBuildResult
            {
                Root = root,
                Thief = thief,
                Dog = dog,
                Owner = owner,
                Match = match,
                NavMesh = environment.NavMesh,
                MainCamera = camera,
                Hud = hud
            };
        }

        private static Scene OpenOrCreateScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GreyboxAssets.EnsureFolder(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            return scene;
        }

        private static void ApplyNightAmbient()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = NightAmbient;
            RenderSettings.skybox = null;
        }

        // Lưu dữ liệu NavMesh thành asset để scene giữ được kết quả bake sau khi đóng mở lại.
        private static void BakeNavMesh(NavMeshSurface surface)
        {
            surface.BuildNavMesh();
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshDataPath) != null)
            {
                AssetDatabase.DeleteAsset(NavMeshDataPath);
            }

            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshDataPath);
        }

        private static void RemoveExistingRoot(Scene scene)
        {
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                if (gameObject.name == RootName)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }
    }
}
