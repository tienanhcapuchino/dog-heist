using System.Text.RegularExpressions;
using DogHeist.Gameplay.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DogHeist.Tests.EditMode
{
    public sealed class AudioCueTests
    {
        private AudioCue _cue;

        [SetUp]
        public void Create() => _cue = ScriptableObject.CreateInstance<AudioCue>();

        [TearDown]
        public void Destroy() => Object.DestroyImmediate(_cue);

        [Test]
        public void Prepare_NeverRepeatsPreviousClip()
        {
            var a = AudioClip.Create("a", 100, 1, 44100, false);
            var b = AudioClip.Create("b", 100, 1, 44100, false);
            _cue.SetClips(new[] { a, b });

            var first = _cue.Prepare(_ => 0, () => 0.5f).Clip;
            var second = _cue.Prepare(_ => 0, () => 0.5f).Clip;

            Assert.AreSame(a, first);
            Assert.AreSame(b, second);
        }

        [Test]
        public void Prepare_VolumeAndPitchStayInRange()
        {
            _cue.SetClips(new[] { AudioClip.Create("a", 100, 1, 44100, false) });

            var low = _cue.Prepare(_ => 0, () => 0f);
            var high = _cue.Prepare(_ => 0, () => 1f);

            Assert.AreEqual(0.9f, low.Volume, 1e-4f);
            Assert.AreEqual(0.95f, low.Pitch, 1e-4f);
            Assert.AreEqual(1f, high.Volume, 1e-4f);
            Assert.AreEqual(1.05f, high.Pitch, 1e-4f);
        }

        [Test]
        public void Prepare_EmptyCue_ReturnsNoClipAndWarnsOnce()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Audio\]"));

            Assert.IsNull(_cue.Prepare().Clip);
            Assert.IsNull(_cue.Prepare().Clip);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
