using System;
using System.IO;
using System.Text.RegularExpressions;
using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DogHeist.Tests.EditMode
{
    public sealed class JsonFileStatsStorageTests
    {
        private string _directory;
        private string _filePath;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "DogHeistTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _filePath = Path.Combine(_directory, "player_stats.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        [Test]
        public void Load_WhenFileMissing_ReturnsEmptyStats()
        {
            var stats = new JsonFileStatsStorage(_filePath).Load();

            Assert.AreEqual(0, stats.SuccessfulHeists);
            Assert.AreEqual(PlayerStats.CurrentSaveVersion, stats.SaveVersion);
        }

        [Test]
        public void SaveThenLoad_RoundTripsValues()
        {
            var service = new StatsService(new JsonFileStatsStorage(_filePath));
            service.Record(new MatchResult(PlayerRole.Thief, MatchOutcome.ThiefEscaped, 42f, false));

            var reloaded = new JsonFileStatsStorage(_filePath).Load();

            Assert.AreEqual(1, reloaded.SuccessfulHeists);
            Assert.AreEqual(1, reloaded.CleanHeists);
            Assert.AreEqual(1, reloaded.BestHeistStreak);
        }

        [Test]
        public void Load_WhenFileCorrupted_ReturnsEmptyStatsAndKeepsBackup()
        {
            File.WriteAllText(_filePath, "day khong phai json");
            LogAssert.Expect(LogType.Error, new Regex(@"\[Stats\]"));

            var stats = new JsonFileStatsStorage(_filePath).Load();

            Assert.AreEqual(0, stats.SuccessfulHeists);
            Assert.IsTrue(File.Exists(_filePath + ".corrupted"));
        }
    }
}
