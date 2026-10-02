using DogHeist.Gameplay.Noise;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class NoiseEventTests
    {
        [Test]
        public void IsAudibleFrom_InsideRadius_ReturnsTrue()
        {
            var noise = new NoiseEvent(Vector3.zero, 5f, NoiseSource.Thief, null);

            Assert.IsTrue(noise.IsAudibleFrom(new Vector3(3f, 0f, 0f)));
        }

        [Test]
        public void IsAudibleFrom_OutsideRadius_ReturnsFalse()
        {
            var noise = new NoiseEvent(Vector3.zero, 5f, NoiseSource.Thief, null);

            Assert.IsFalse(noise.IsAudibleFrom(new Vector3(6f, 0f, 0f)));
        }

        [Test]
        public void IsAudibleFrom_LowSensitivity_ShrinksHearingRange()
        {
            var noise = new NoiseEvent(Vector3.zero, 5f, NoiseSource.Thief, null);

            Assert.IsFalse(noise.IsAudibleFrom(new Vector3(3f, 0f, 0f), sensitivity: 0.5f));
        }

        [Test]
        public void IsAudibleFrom_ZeroRadius_IsNeverAudible()
        {
            var noise = new NoiseEvent(Vector3.zero, 0f, NoiseSource.Thief, null);

            Assert.IsFalse(noise.IsAudibleFrom(Vector3.zero));
        }
    }
}
