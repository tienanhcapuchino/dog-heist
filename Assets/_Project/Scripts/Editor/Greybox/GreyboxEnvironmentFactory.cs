using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Stealth;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Dựng mặt đất, hàng rào, nhà, chuồng chó, bụi cây, đèn, vùng sáng, vùng thoát và các điểm mốc.
    /// NavMeshSurface gắn lên chính "Environment" và chỉ thu thập con của nó, nên nhân vật không bị tính là vật cản.
    /// </summary>
    internal static class GreyboxEnvironmentFactory
    {
        private const float PlaneSize = 10f; // Plane mặc định của Unity rộng 10 x 10 m.
        private const float BoundaryThickness = 0.5f;

        public static GreyboxEnvironment Build(Transform root, GreyboxAssets assets)
        {
            var environment = GreyboxPrimitives.CreateEmpty("Environment", root, Vector3.zero).transform;

            var groundScale = GreyboxLayout.GroundSize / PlaneSize;
            GreyboxPrimitives.Create(PrimitiveType.Plane, "Ground", environment, Vector3.zero,
                new Vector3(groundScale, 1f, groundScale), assets.GroundMaterial);

            BuildFences(environment, assets);
            BuildBoundary(environment, assets);
            GreyboxPrimitives.Create(PrimitiveType.Cube, "House", environment,
                GreyboxLayout.HouseCenter, GreyboxLayout.HouseSize, assets.BuildingMaterial);
            GreyboxPrimitives.Create(PrimitiveType.Cube, "Kennel", environment,
                GreyboxLayout.KennelCenter, GreyboxLayout.KennelSize, assets.BuildingMaterial);
            BuildBushes(environment, assets);
            BuildLights(environment);

            GreyboxPrimitives.CreateTrigger("LightZone", environment,
                GreyboxLayout.LightZoneCenter, GreyboxLayout.LightZoneSize).AddComponent<LightZone>();
            GreyboxPrimitives.CreateTrigger("EscapeZone", environment,
                GreyboxLayout.EscapeZoneCenter, GreyboxLayout.EscapeZoneSize).AddComponent<EscapeZone>();

            var surface = environment.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;

            return new GreyboxEnvironment
            {
                DogHome = GreyboxPrimitives.CreateEmpty("DogHome", root, GreyboxLayout.DogHome).transform,
                OwnerBed = GreyboxPrimitives.CreateEmpty("OwnerBed", root, GreyboxLayout.OwnerBed).transform,
                PatrolWaypoints = BuildWaypoints(root),
                NavMesh = surface
            };
        }

        private static void BuildFences(Transform parent, GreyboxAssets assets)
        {
            var segments = GreyboxLayout.BuildFenceSegments();
            for (var i = 0; i < segments.Count; i++)
            {
                GreyboxPrimitives.Create(PrimitiveType.Cube, $"Fence{i:00}", parent,
                    segments[i].Center, segments[i].Size, assets.FenceMaterial);
            }
        }

        // Tường vô hình quanh mép mặt đất để người chơi không rơi khỏi màn.
        private static void BuildBoundary(Transform parent, GreyboxAssets assets)
        {
            var half = GreyboxLayout.GroundSize * 0.5f;
            var height = GreyboxLayout.BoundaryHeight;
            var y = height * 0.5f;
            var specs = new[]
            {
                (new Vector3(0f, y, half), new Vector3(GreyboxLayout.GroundSize, height, BoundaryThickness)),
                (new Vector3(0f, y, -half), new Vector3(GreyboxLayout.GroundSize, height, BoundaryThickness)),
                (new Vector3(half, y, 0f), new Vector3(BoundaryThickness, height, GreyboxLayout.GroundSize)),
                (new Vector3(-half, y, 0f), new Vector3(BoundaryThickness, height, GreyboxLayout.GroundSize))
            };

            for (var i = 0; i < specs.Length; i++)
            {
                var (center, size) = specs[i];
                var wall = GreyboxPrimitives.Create(PrimitiveType.Cube, $"Boundary{i}", parent, center, size, assets.FenceMaterial);
                Object.DestroyImmediate(wall.GetComponent<MeshRenderer>());
            }
        }

        // Bụi cây: khối cầu chỉ để nhìn (không chặn người) cộng trigger HidingSpot để nấp.
        private static void BuildBushes(Transform parent, GreyboxAssets assets)
        {
            for (var i = 0; i < GreyboxLayout.Bushes.Length; i++)
            {
                var bush = GreyboxLayout.Bushes[i];
                var position = bush.Position + Vector3.up * (bush.Diameter * 0.4f);
                var gameObject = GreyboxPrimitives.Create(PrimitiveType.Sphere, $"Bush{i}", parent, position,
                    Vector3.one * bush.Diameter, assets.BushMaterial, keepCollider: false);

                var trigger = gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                gameObject.AddComponent<HidingSpot>();
            }
        }

        private static void BuildLights(Transform parent)
        {
            var moon = GreyboxPrimitives.CreateEmpty("Moonlight", parent, new Vector3(0f, 10f, 0f));
            moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var moonLight = moon.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.6f, 0.7f, 1f);
            moonLight.intensity = 0.25f;
            moonLight.shadows = LightShadows.Soft;

            var porch = GreyboxPrimitives.CreateEmpty("PorchLight", parent, GreyboxLayout.PorchLightPosition);
            porch.transform.LookAt(GreyboxLayout.PorchLightTarget);
            var porchLight = porch.AddComponent<Light>();
            porchLight.type = LightType.Spot;
            porchLight.color = new Color(1f, 0.85f, 0.6f);
            porchLight.intensity = 4f;
            porchLight.range = 14f;
            porchLight.spotAngle = 70f;
            porchLight.shadows = LightShadows.Soft;
        }

        private static Transform[] BuildWaypoints(Transform root)
        {
            var group = GreyboxPrimitives.CreateEmpty("PatrolWaypoints", root, Vector3.zero).transform;
            var waypoints = new Transform[GreyboxLayout.PatrolWaypoints.Length];
            for (var i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = GreyboxPrimitives.CreateEmpty($"Waypoint{i}", group, GreyboxLayout.PatrolWaypoints[i]).transform;
            }

            return waypoints;
        }
    }
}
