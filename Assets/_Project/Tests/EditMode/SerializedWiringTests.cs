using System;
using DogHeist.AI.Owner;
using DogHeist.EditorTools.Greybox;
using DogHeist.Gameplay.Thief;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DogHeist.Tests.EditMode
{
    public sealed class SerializedWiringTests
    {
        private GameObject _host;

        [SetUp]
        public void CreateHost() => _host = new GameObject("WiringTestHost");

        [TearDown]
        public void DestroyHost() => Object.DestroyImmediate(_host);

        [Test]
        public void Assign_SetsPrivateSerializedField()
        {
            var motor = _host.AddComponent<ThiefMotor>();
            var config = ScriptableObject.CreateInstance<ThiefConfig>();

            SerializedWiring.Assign(motor, "_config", config);

            Assert.AreSame(config, motor.Config);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Assign_UnknownField_Throws()
        {
            var motor = _host.AddComponent<ThiefMotor>();

            var error = Assert.Throws<ArgumentException>(() => SerializedWiring.Assign(motor, "_doesNotExist", null));
            StringAssert.StartsWith("[Greybox]", error.Message);
        }

        [Test]
        public void Assign_WrongType_Throws()
        {
            var motor = _host.AddComponent<ThiefMotor>();

            Assert.Throws<ArgumentException>(() => SerializedWiring.Assign(motor, "_config", _host.transform));
        }

        [Test]
        public void AssignArray_SetsAllElements()
        {
            var owner = _host.AddComponent<OwnerAI>();
            var first = new GameObject("WaypointA").transform;
            var second = new GameObject("WaypointB").transform;

            SerializedWiring.AssignArray(owner, "_patrolWaypoints", new Object[] { first, second });

            var property = new SerializedObject(owner).FindProperty("_patrolWaypoints");
            Assert.AreEqual(2, property.arraySize);
            Assert.AreSame(second, property.GetArrayElementAtIndex(1).objectReferenceValue);
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }
    }
}
