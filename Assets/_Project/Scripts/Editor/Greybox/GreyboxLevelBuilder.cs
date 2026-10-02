using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using UnityEngine;
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

            environment.NavMesh.layerMask = ~(1 << assets.CharactersLayer);

            return new GreyboxBuildResult
            {
                Root = root,
                Thief = thief,
                Dog = dog,
                Owner = owner,
                Match = match,
                NavMesh = environment.NavMesh
            };
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
