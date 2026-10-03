using System.Collections.Generic;
using DogHeist.AI.Dog;
using DogHeist.AI.Owner;
using DogHeist.Gameplay.Awareness;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class AwarenessTests
    {
        [Test]
        public void Tracker_RaisesOnlyWhenLevelChanges()
        {
            var tracker = new AwarenessTracker();
            var raised = new List<AwarenessLevel>();
            tracker.Changed += raised.Add;

            tracker.Set(AwarenessLevel.None);
            tracker.Set(AwarenessLevel.Suspicious);
            tracker.Set(AwarenessLevel.Suspicious);
            tracker.Set(AwarenessLevel.Alerted);

            CollectionAssert.AreEqual(new[] { AwarenessLevel.Suspicious, AwarenessLevel.Alerted }, raised);
            Assert.AreEqual(AwarenessLevel.Alerted, tracker.Current);
        }

        [Test]
        public void DogStates_DeclareSpecLevels()
        {
            var dog = new GameObject("Dog").AddComponent<DogAI>();
            try
            {
                Assert.AreEqual(AwarenessLevel.None, new DogIdleState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Alerted, new DogAlertState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Friendly, new DogEatLureState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Friendly, new DogCalmState(dog).Awareness);
                Assert.AreEqual(AwarenessLevel.Friendly, new DogCarriedState(dog).Awareness);
            }
            finally
            {
                Object.DestroyImmediate(dog.gameObject);
            }
        }

        [Test]
        public void OwnerStates_DeclareSpecLevels()
        {
            var owner = new GameObject("Owner").AddComponent<OwnerAI>();
            try
            {
                Assert.AreEqual(AwarenessLevel.Sleeping, new OwnerSleepingState(owner).Awareness);
                Assert.AreEqual(AwarenessLevel.Suspicious, new OwnerInvestigateState(owner).Awareness);
                Assert.AreEqual(AwarenessLevel.Suspicious, new OwnerPatrolState(owner).Awareness);
                Assert.AreEqual(AwarenessLevel.Alerted, new OwnerChaseState(owner).Awareness);
            }
            finally
            {
                Object.DestroyImmediate(owner.gameObject);
            }
        }

        [Test]
        public void AiIndicatorAnchor_FallsBackToOwnTransform()
        {
            var dog = new GameObject("Dog").AddComponent<DogAI>();
            try
            {
                Assert.AreSame(dog.transform, ((IAwarenessSource)dog).IndicatorAnchor);
            }
            finally
            {
                Object.DestroyImmediate(dog.gameObject);
            }
        }
    }
}
