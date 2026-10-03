using System.Collections.Generic;
using DogHeist.Gameplay.Audio;
using DogHeist.Gameplay.Awareness;
using NUnit.Framework;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class SoundRulesTests
    {
        private readonly List<Object> _created = new List<Object>();
        private SoundLibrary _library;

        [SetUp]
        public void Create()
        {
            _library = ScriptableObject.CreateInstance<SoundLibrary>();
            _created.Add(_library);
            foreach (var name in new[] { "FootstepCrouch", "FootstepWalk", "FootstepSprint", "DogHappy", "OwnerHuh", "OwnerShout" })
            {
                var cue = ScriptableObject.CreateInstance<AudioCue>();
                cue.name = name;
                _library.Assign(name, cue);
                _created.Add(cue);
            }
        }

        [TearDown]
        public void Destroy()
        {
            foreach (var created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [TestCase(false, false, "FootstepWalk")]
        [TestCase(true, false, "FootstepCrouch")]
        [TestCase(false, true, "FootstepSprint")]
        [TestCase(true, true, "FootstepCrouch")]
        public void Footstep_PicksByMovement(bool crouching, bool sprinting, string expected)
        {
            Assert.AreEqual(expected, SoundRules.Footstep(_library, crouching, sprinting).name);
        }

        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Sleeping, AwarenessLevel.Suspicious, "OwnerHuh")]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Suspicious, AwarenessLevel.Alerted, "OwnerShout")]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Sleeping, AwarenessLevel.Alerted, "OwnerShout")]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.Alerted, AwarenessLevel.Friendly, "DogHappy")]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.None, AwarenessLevel.Friendly, "DogHappy")]
        public void AwarenessChange_PlaysExpectedCue(AwarenessVoice voice, AwarenessLevel from, AwarenessLevel to, string expected)
        {
            Assert.AreEqual(expected, SoundRules.ForAwarenessChange(_library, voice, from, to).name);
        }

        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Alerted, AwarenessLevel.Suspicious)]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.None, AwarenessLevel.Sleeping)]
        [TestCase(AwarenessVoice.Owner, AwarenessLevel.Suspicious, AwarenessLevel.Sleeping)]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.Friendly, AwarenessLevel.Friendly)]
        [TestCase(AwarenessVoice.Dog, AwarenessLevel.None, AwarenessLevel.Alerted)]
        public void AwarenessChange_SilentCases(AwarenessVoice voice, AwarenessLevel from, AwarenessLevel to)
        {
            Assert.IsNull(SoundRules.ForAwarenessChange(_library, voice, from, to));
        }
    }
}
