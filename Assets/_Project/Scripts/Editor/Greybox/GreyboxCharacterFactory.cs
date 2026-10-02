using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.AI.Sensors;
using DogHeist.Gameplay.Controls;
using DogHeist.Gameplay.Dog;
using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.AI;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Tạo trộm, chó, chủ nhà bằng khối capsule và nối tham chiếu như bước 9–15 trong README.
    /// Gốc mỗi nhân vật đặt ở chân (y = 0), thân capsule là object con.
    /// </summary>
    internal static class GreyboxCharacterFactory
    {
        public static ThiefMotor CreateThief(Transform parent, GreyboxAssets assets)
        {
            var thief = GreyboxPrimitives.CreateEmpty("Thief", parent, GreyboxLayout.ThiefSpawn);
            thief.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Nhìn về phía lỗ hàng rào.

            var controller = thief.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            GreyboxPrimitives.Create(PrimitiveType.Capsule, "Body", thief.transform, new Vector3(0f, 0.9f, 0f),
                new Vector3(0.7f, 0.9f, 0.7f), assets.ThiefMaterial, keepCollider: false);

            thief.AddComponent<LocalPlayerInput>();
            var motor = thief.AddComponent<ThiefMotor>();
            var visibility = thief.AddComponent<ThiefVisibility>();
            var interactor = thief.AddComponent<ThiefInteractor>();
            var carryAnchor = GreyboxPrimitives.CreateEmpty("CarryAnchor", thief.transform, new Vector3(0f, 1.1f, 0.5f));
            var throwOrigin = GreyboxPrimitives.CreateEmpty("ThrowOrigin", thief.transform, new Vector3(0f, 1.4f, 0.7f));

            SerializedWiring.Assign(motor, "_config", assets.ThiefConfig);
            SerializedWiring.Assign(visibility, "_motor", motor);
            SerializedWiring.Assign(interactor, "_motor", motor);
            SerializedWiring.Assign(interactor, "_carryAnchor", carryAnchor.transform);
            SerializedWiring.Assign(interactor, "_throwOrigin", throwOrigin.transform);
            SerializedWiring.Assign(interactor, "_lurePrefab", assets.LurePrefab);

            GreyboxPrimitives.SetLayerRecursively(thief, assets.CharactersLayer);
            return motor;
        }

        public static DogAI CreateDog(Transform parent, GreyboxAssets assets, ThiefVisibility thief, Transform home)
        {
            var dog = GreyboxPrimitives.CreateEmpty("Dog", parent, home.localPosition);

            var agent = dog.AddComponent<NavMeshAgent>();
            agent.radius = 0.3f;
            agent.height = 0.6f;
            // Giữ collider của thân để trộm tìm thấy chó khi bấm E.
            GreyboxPrimitives.Create(PrimitiveType.Capsule, "Body", dog.transform, new Vector3(0f, 0.3f, 0f),
                new Vector3(0.5f, 0.3f, 0.5f), assets.DogMaterial);

            dog.AddComponent<CarryableDog>();
            var hearing = dog.AddComponent<HearingSensor>();
            var ai = dog.AddComponent<DogAI>();

            SerializedWiring.Assign(ai, "_config", assets.DogConfig);
            SerializedWiring.Assign(ai, "_hearing", hearing);
            SerializedWiring.Assign(ai, "_thief", thief);
            SerializedWiring.Assign(ai, "_home", home);

            GreyboxPrimitives.SetLayerRecursively(dog, assets.CharactersLayer);
            return ai;
        }

        public static OwnerAI CreateOwner(
            Transform parent,
            GreyboxAssets assets,
            ThiefVisibility thief,
            Transform bed,
            Transform[] patrolWaypoints)
        {
            var owner = GreyboxPrimitives.CreateEmpty("Owner", parent, bed.localPosition);
            owner.transform.rotation = Quaternion.Euler(0f, 225f, 0f); // Nhìn ra sân về phía chuồng chó.

            var agent = owner.AddComponent<NavMeshAgent>();
            agent.radius = 0.4f;
            agent.height = 1.9f;
            GreyboxPrimitives.Create(PrimitiveType.Capsule, "Body", owner.transform, new Vector3(0f, 0.95f, 0f),
                new Vector3(0.8f, 0.95f, 0.8f), assets.OwnerMaterial);
            var eye = GreyboxPrimitives.CreateEmpty("Eye", owner.transform, new Vector3(0f, 1.6f, 0f));

            var hearing = owner.AddComponent<HearingSensor>();
            var vision = owner.AddComponent<VisionSensor>();
            var ai = owner.AddComponent<OwnerAI>();

            SerializedWiring.Assign(vision, "_eye", eye.transform);
            SerializedWiring.Assign(ai, "_config", assets.OwnerConfig);
            SerializedWiring.Assign(ai, "_hearing", hearing);
            SerializedWiring.Assign(ai, "_vision", vision);
            SerializedWiring.Assign(ai, "_thief", thief);
            SerializedWiring.Assign(ai, "_bed", bed);
            SerializedWiring.AssignArray(ai, "_patrolWaypoints", patrolWaypoints);

            GreyboxPrimitives.SetLayerRecursively(owner, assets.CharactersLayer);
            return ai;
        }
    }
}
