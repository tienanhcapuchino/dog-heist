using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using NUnit.Framework;

namespace DogHeist.Tests.EditMode
{
    public sealed class StatsServiceTests
    {
        [Test]
        public void Escape_IncrementsSuccessfulHeistsAndStreak()
        {
            var service = new StatsService(new InMemoryStatsStorage());

            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped));
            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped));

            Assert.AreEqual(2, service.Current.SuccessfulHeists);
            Assert.AreEqual(2, service.Current.CurrentHeistStreak);
            Assert.AreEqual(2, service.Current.BestHeistStreak);
        }

        [Test]
        public void Caught_ResetsCurrentStreakButKeepsBestStreak()
        {
            var service = new StatsService(new InMemoryStatsStorage());

            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped));
            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped));
            service.Record(ThiefMatch(MatchOutcome.ThiefCaught));

            Assert.AreEqual(1, service.Current.TimesCaught);
            Assert.AreEqual(0, service.Current.CurrentHeistStreak);
            Assert.AreEqual(2, service.Current.BestHeistStreak);
        }

        [Test]
        public void EscapeWithoutBeingSpotted_CountsAsCleanHeist()
        {
            var service = new StatsService(new InMemoryStatsStorage());

            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped, wasSpotted: false));
            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped, wasSpotted: true));

            Assert.AreEqual(2, service.Current.SuccessfulHeists);
            Assert.AreEqual(1, service.Current.CleanHeists);
        }

        [Test]
        public void OwnerRole_CatchingThief_IncrementsThievesCaught()
        {
            var service = new StatsService(new InMemoryStatsStorage());

            service.Record(new MatchResult(PlayerRole.Owner, MatchOutcome.ThiefCaught, 30f, true));

            Assert.AreEqual(1, service.Current.ThievesCaught);
            Assert.AreEqual(0, service.Current.TimesCaught);
        }

        [Test]
        public void Record_SavesToStorage_ButIgnoresOutcomeNone()
        {
            var storage = new InMemoryStatsStorage();
            var service = new StatsService(storage);

            service.Record(ThiefMatch(MatchOutcome.None));
            service.Record(ThiefMatch(MatchOutcome.ThiefEscaped));

            Assert.AreEqual(1, storage.SaveCount);
        }

        private static MatchResult ThiefMatch(MatchOutcome outcome, bool wasSpotted = true) =>
            new(PlayerRole.Thief, outcome, 60f, wasSpotted);

        private sealed class InMemoryStatsStorage : IStatsStorage
        {
            private PlayerStats _stored = new();

            public int SaveCount { get; private set; }

            public PlayerStats Load() => _stored;

            public void Save(PlayerStats stats)
            {
                _stored = stats;
                SaveCount++;
            }
        }
    }
}
