using System;
using System.IO;
using System.Text.RegularExpressions;
using DogHeist.Core.Match;
using DogHeist.Core.MatchLog;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DogHeist.Tests.EditMode
{
    public sealed class MatchLogWriterTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "DogHeistMatchLog_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [Test]
        public void Fingerprint_IsStableEightHex()
        {
            var first = ConfigFingerprint.Compute(new[] { "{\"a\":1}", "{\"b\":2}" });
            var second = ConfigFingerprint.Compute(new[] { "{\"a\":1}", "{\"b\":2}" });

            Assert.AreEqual(first, second);
            StringAssert.IsMatch("^[0-9a-f]{8}$", first);
        }

        [Test]
        public void Fingerprint_ChangesWhenAnyValueChanges() =>
            Assert.AreNotEqual(ConfigFingerprint.Compute(new[] { "{\"a\":1}" }), ConfigFingerprint.Compute(new[] { "{\"a\":2}" }));

        [Test]
        public void Fingerprint_PartBoundariesMatter() =>
            Assert.AreNotEqual(ConfigFingerprint.Compute(new[] { "ab", "c" }), ConfigFingerprint.Compute(new[] { "a", "bc" }));

        [Test]
        public void Append_TwiceWritesTwoJsonLines()
        {
            var writer = new JsonLinesMatchLogWriter(Path.Combine(_tempDir, "sub", "match_log.jsonl"));
            writer.Append(new MatchLogBuilder().Build(MatchOutcome.ThiefCaught, 10f, "h1", DateTime.UtcNow));
            writer.Append(new MatchLogBuilder().Build(MatchOutcome.ThiefEscaped, 20f, "h2", DateTime.UtcNow));

            var lines = File.ReadAllLines(writer.FilePath);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual(20f, JsonUtility.FromJson<MatchLogEntry>(lines[1]).DurationSeconds);
        }

        [Test]
        public void Append_UnwritablePath_LogsAndDoesNotThrow()
        {
            var blockingFile = Path.Combine(_tempDir, "not_a_folder");
            File.WriteAllText(blockingFile, "x");
            var writer = new JsonLinesMatchLogWriter(Path.Combine(blockingFile, "match_log.jsonl"));
            LogAssert.Expect(LogType.Error, new Regex(@"\[MatchLog\]"));

            Assert.DoesNotThrow(() => writer.Append(new MatchLogBuilder().Build(MatchOutcome.ThiefCaught, 1f, "h", DateTime.UtcNow)));
        }
    }
}
