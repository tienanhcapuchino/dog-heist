using System.Linq;
using DogHeist.EditorTools.Greybox;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Stealth;
using DogHeist.Gameplay.Thief;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxLevelBuilderTests : GreyboxSceneTestBase
    {
        [Test]
        public void BuildInto_WiresThief()
        {
            var result = Build();
            var interactor = result.Thief.GetComponent<ThiefInteractor>();

            Assert.IsNotNull(result.Thief.Config);
            Assert.IsNotNull(ReadReference(interactor, "_lurePrefab"));
            Assert.IsNotNull(ReadReference(interactor, "_carryAnchor"));
            Assert.IsNotNull(ReadReference(interactor, "_throwOrigin"));
            Assert.AreSame(result.Thief, ReadReference(result.Thief.GetComponent<ThiefVisibility>(), "_motor"));
        }

        [Test]
        public void BuildInto_WiresDogAndOwnerToThief()
        {
            var result = Build();
            var visibility = result.Thief.GetComponent<ThiefVisibility>();

            Assert.AreSame(visibility, ReadReference(result.Dog, "_thief"));
            Assert.IsNotNull(ReadReference(result.Dog, "_hearing"));
            Assert.IsNotNull(ReadReference(result.Dog, "_home"));
            Assert.AreSame(visibility, ReadReference(result.Owner, "_thief"));
            Assert.IsNotNull(ReadReference(result.Owner, "_vision"));
            Assert.IsNotNull(ReadReference(result.Owner, "_bed"));
            var waypoints = new SerializedObject(result.Owner).FindProperty("_patrolWaypoints");
            Assert.AreEqual(GreyboxLayout.PatrolWaypoints.Length, waypoints.arraySize);
        }

        [Test]
        public void BuildInto_MatchManagerReferencesThief()
        {
            var result = Build();

            Assert.AreSame(result.Thief, ReadReference(result.Match, "_thief"));
        }

        [Test]
        public void BuildInto_PutsCharactersAndChildrenOnCharactersLayer()
        {
            var result = Build();
            var layer = LayerMask.NameToLayer(GreyboxAssets.CharactersLayerName);

            foreach (var character in new Component[] { result.Thief, result.Dog, result.Owner })
            {
                Assert.IsTrue(character.GetComponentsInChildren<Transform>().All(t => t.gameObject.layer == layer), character.name);
            }
        }

        [Test]
        public void BuildInto_CreatesFencesHidingSpotsAndZones()
        {
            var result = Build();

            var fenceCount = result.Root.GetComponentsInChildren<Transform>().Count(t => t.name.StartsWith("Fence"));
            Assert.AreEqual(GreyboxLayout.BuildFenceSegments().Count, fenceCount);
            Assert.AreEqual(GreyboxLayout.Bushes.Length, result.Root.GetComponentsInChildren<HidingSpot>().Length);
            Assert.IsTrue(result.Root.GetComponentInChildren<LightZone>().GetComponent<Collider>().isTrigger);
            Assert.IsTrue(result.Root.GetComponentInChildren<EscapeZone>().GetComponent<Collider>().isTrigger);
        }

        [Test]
        public void BuildInto_NavMeshCollectsEnvironmentOnlyAndExcludesCharacters()
        {
            var result = Build();
            var layer = LayerMask.NameToLayer(GreyboxAssets.CharactersLayerName);

            Assert.AreEqual(CollectObjects.Children, result.NavMesh.collectObjects);
            Assert.AreEqual(0, result.NavMesh.layerMask.value & (1 << layer));
        }

        [Test]
        public void BuildInto_CalledTwice_KeepsSingleRootAndUserObjects()
        {
            var userObject = new GameObject("UserAddedProp");

            Build();
            Build();

            Assert.AreEqual(1, Scene.GetRootGameObjects().Count(go => go.name == GreyboxLevelBuilder.RootName));
            Assert.IsTrue(userObject != null);
        }
    }
}
