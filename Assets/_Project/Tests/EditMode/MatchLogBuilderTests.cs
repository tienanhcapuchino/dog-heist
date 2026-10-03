using System;
using DogHeist.Core.Match;
using DogHeist.Core.MatchLog;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class MatchLogBuilderTests
    {
        [Test]
        public void MarkFirst_KeepsOnlyFirstTime()
        {
            var builder = new MatchLogBuilder();
            builder.MarkFirst(MatchMilestone.FirstBark, 12.5f);
            builder.MarkFirst(MatchMilestone.FirstBark, 40f);

            Assert.AreEqual(12.5f, builder.GetFirst(MatchMilestone.FirstBark));
        }

        [Test]
        public void Build_UnreachedMilestonesAreMinusOne()
        {
            var entry = new MatchLogBuilder().Build(
                MatchOutcome.ThiefCaught, 30f, "abcd1234", new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(-1f, entry.FirstBarkSeconds);
            Assert.AreEqual(-1f, entry.FirstPickupSeconds);
            Assert.AreEqual("ThiefCaught", entry.Outcome);
            Assert.AreEqual("abcd1234", entry.ConfigHash);
            Assert.AreEqual("2026-10-03T00:00:00.0000000Z", entry.EndedAtUtc);
        }

        [Test]
        public void Build_CopiesCountersAndTimes()
        {
            var builder = new MatchLogBuilder();
            builder.Increment(MatchCounter.Bark);
            builder.Increment(MatchCounter.Bark);
            builder.Increment(MatchCounter.LureThrown);
            builder.AddTime(ThiefStateTime.InLight, 1.5f);
            builder.AddTime(ThiefStateTime.InLight, 2f);
            builder.AddTime(ThiefStateTime.Hidden, -1f);
            builder.MarkFirst(MatchMilestone.Pickup, 90f);

            var entry = builder.Build(MatchOutcome.ThiefEscaped, 200f, "h", DateTime.UtcNow);

            Assert.AreEqual(2, entry.BarkCount);
            Assert.AreEqual(1, entry.LuresThrown);
            Assert.AreEqual(3.5f, entry.SecondsInLight, 1e-4f);
            Assert.AreEqual(0f, entry.SecondsHidden);
            Assert.AreEqual(90f, entry.FirstPickupSeconds);
            Assert.AreEqual(200f, entry.DurationSeconds);
        }

        [Test]
        public void Entry_SerializesWithUnderscoreKeys()
        {
            var builder = new MatchLogBuilder();
            builder.Increment(MatchCounter.Bark);
            var json = JsonUtility.ToJson(builder.Build(MatchOutcome.ThiefEscaped, 1f, "h", DateTime.UtcNow));

            StringAssert.Contains("\"_barkCount\":1", json);
            StringAssert.Contains("\"_outcome\":\"ThiefEscaped\"", json);
        }
    }
}
