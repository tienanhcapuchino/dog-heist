using System.Collections.Generic;
using DogHeist.Core.Match;
using DogHeist.Core.MatchLog;
using DogHeist.Gameplay.Awareness;
using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Noise;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class MatchLogRecorderTests
    {
        private sealed class FakeWriter : IMatchLogWriter
        {
            public List<MatchLogEntry> Entries { get; } = new List<MatchLogEntry>();

            public void Append(MatchLogEntry entry) => Entries.Add(entry);
        }

        private MatchLogRecorder _recorder;
        private FakeWriter _writer;
        private float _time;

        [SetUp]
        public void SetUp()
        {
            _time = 0f;
            _writer = new FakeWriter();
            _recorder = new GameObject("Recorder").AddComponent<MatchLogRecorder>();
            _recorder.Writer = _writer;
            _recorder.Clock = () => _time;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_recorder.gameObject);

        private static MatchResult Result(MatchOutcome outcome, float duration) =>
            new MatchResult(PlayerRole.Thief, outcome, duration, false);

        [Test]
        public void MatchEnded_Twice_WritesOneLine()
        {
            _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 42f), null);
            _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 42f), null);

            Assert.AreEqual(1, _writer.Entries.Count);
            Assert.AreEqual(42f, _writer.Entries[0].DurationSeconds);
        }

        [Test]
        public void NoReferences_StillWritesWithUnreachedMilestones()
        {
            _recorder.Sample(1f);
            _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 5f), null);

            Assert.AreEqual(-1f, _writer.Entries[0].FirstOwnerWakeSeconds);
            StringAssert.IsMatch("^[0-9a-f]{8}$", _writer.Entries[0].ConfigHash);
        }

        [Test]
        public void OwnerWake_CountsEachLeaveFromSleeping()
        {
            _recorder.SetOwnerLevelForTests(AwarenessLevel.Sleeping);
            _time = 10f; _recorder.HandleOwnerAwareness(AwarenessLevel.Suspicious);
            _recorder.HandleOwnerAwareness(AwarenessLevel.Alerted);
            _recorder.HandleOwnerAwareness(AwarenessLevel.Suspicious);
            _recorder.HandleOwnerAwareness(AwarenessLevel.Sleeping);
            _time = 50f; _recorder.HandleOwnerAwareness(AwarenessLevel.Alerted);
            _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefCaught, 60f), null);

            Assert.AreEqual(2, _writer.Entries[0].OwnerWakeCount);
            Assert.AreEqual(10f, _writer.Entries[0].FirstOwnerWakeSeconds);
        }

        [Test]
        public void DogBarks_CountedOnlyForDogNoise()
        {
            _time = 7f;
            _recorder.HandleNoise(new NoiseEvent(Vector3.zero, 16f, NoiseSource.Dog, null));
            _recorder.HandleNoise(new NoiseEvent(Vector3.zero, 4f, NoiseSource.Thief, null));
            _recorder.HandleNoise(new NoiseEvent(Vector3.zero, 16f, NoiseSource.Dog, null));
            _recorder.HandleMatchEnded(Result(MatchOutcome.ThiefEscaped, 100f), null);

            Assert.AreEqual(2, _writer.Entries[0].BarkCount);
            Assert.AreEqual(7f, _writer.Entries[0].FirstBarkSeconds);
        }
    }
}
