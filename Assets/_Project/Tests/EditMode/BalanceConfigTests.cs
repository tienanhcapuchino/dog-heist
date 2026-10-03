using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.AI.Sensors;
using DogHeist.Gameplay.Items;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class BalanceConfigTests
    {
        [Test]
        public void NewConfigs_DefaultToPreviousValues()
        {
            var owner = ScriptableObject.CreateInstance<OwnerConfig>();
            var dog = ScriptableObject.CreateInstance<DogConfig>();
            try
            {
                Assert.AreEqual(12f, owner.VisionRange);
                Assert.AreEqual(110f, owner.VisionFieldOfView);
                Assert.AreEqual(1f, dog.HearingSensitivity);
                Assert.AreEqual(8f, dog.LureNoticeDistance);
                Assert.AreEqual(4f, dog.EatDuration);
            }
            finally
            {
                Object.DestroyImmediate(owner);
                Object.DestroyImmediate(dog);
            }
        }

        [Test]
        public void VisionSensor_ConfigureOverridesRangeAndFov()
        {
            var sensor = new GameObject("Eye").AddComponent<VisionSensor>();
            try
            {
                sensor.Configure(7f, 400f);
                Assert.AreEqual(7f, sensor.Range);
                Assert.AreEqual(360f, sensor.FieldOfView);
            }
            finally
            {
                Object.DestroyImmediate(sensor.gameObject);
            }
        }

        [Test]
        public void FindNearestUnclaimed_IgnoresLuresBeyondMaxDistance()
        {
            var near = new GameObject("Near").AddComponent<FoodLure>();
            var far = new GameObject("Far").AddComponent<FoodLure>();
            near.transform.position = new Vector3(3f, 0f, 0f);
            far.transform.position = new Vector3(10f, 0f, 0f);
            // EditMode không gọi OnEnable nên đăng ký thủ công qua hàm internal của FoodLure.
            FoodLure.RegisterForTests(near);
            FoodLure.RegisterForTests(far);
            try
            {
                Assert.AreSame(near, FoodLure.FindNearestUnclaimed(Vector3.zero, 5f));
                Assert.IsNull(FoodLure.FindNearestUnclaimed(new Vector3(-10f, 0f, 0f), 5f));
            }
            finally
            {
                FoodLure.UnregisterForTests(near);
                FoodLure.UnregisterForTests(far);
                Object.DestroyImmediate(near.gameObject);
                Object.DestroyImmediate(far.gameObject);
            }
        }
    }
}
