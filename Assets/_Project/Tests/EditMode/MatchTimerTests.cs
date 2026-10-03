using DogHeist.UI.Hud;
using NUnit.Framework;

namespace DogHeist.Tests.EditMode
{
    public sealed class MatchTimerTests
    {
        [TestCase(0f, "0:00")]
        [TestCase(65.4f, "1:05")]
        [TestCase(59.99f, "0:59")]
        [TestCase(600f, "10:00")]
        [TestCase(-3f, "0:00")]
        public void FormatTime_MinutesAndSeconds(float seconds, string expected) =>
            Assert.AreEqual(expected, MatchTimerUI.FormatTime(seconds));
    }
}
