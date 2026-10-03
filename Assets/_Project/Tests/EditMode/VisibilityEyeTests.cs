using DogHeist.UI.Hud;
using NUnit.Framework;

namespace DogHeist.Tests.EditMode
{
    public sealed class VisibilityEyeTests
    {
        [TestCase(0f, 0.08f)]
        [TestCase(1f, 1f)]
        [TestCase(0.5f, 0.54f)]
        [TestCase(-3f, 0.08f)]
        [TestCase(7f, 1f)]
        public void ComputeOpenness_MapsVisibilityIntoRange(float visibility, float expected)
        {
            Assert.AreEqual(expected, VisibilityEyeUI.ComputeOpenness(visibility, 0.08f, 1f), 1e-4f);
        }
    }
}
