using System;
using DogHeist.Gameplay.Awareness;
using DogHeist.UI.Awareness;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DogHeist.Tests.EditMode
{
    public sealed class AwarenessIndicatorTests
    {
        private sealed class FakeSource : IAwarenessSource
        {
            public AwarenessLevel Awareness { get; set; }

            public event Action<AwarenessLevel> AwarenessChanged;

            public Transform IndicatorAnchor { get; set; }

            public void Raise(AwarenessLevel level)
            {
                Awareness = level;
                AwarenessChanged?.Invoke(level);
            }
        }

        private GameObject _host;
        private AwarenessIndicatorStyle _style;

        [SetUp]
        public void Create()
        {
            _host = new GameObject("Indicator");
            _style = ScriptableObject.CreateInstance<AwarenessIndicatorStyle>();
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_style);
        }

        [TestCase(AwarenessLevel.Sleeping, "Zzz")]
        [TestCase(AwarenessLevel.Suspicious, "?")]
        [TestCase(AwarenessLevel.Alerted, "!")]
        [TestCase(AwarenessLevel.Friendly, "♥")]
        public void Style_DefaultTexts(AwarenessLevel level, string expected)
        {
            Assert.IsTrue(_style.TryGet(level, out var text, out _));
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void Style_NoneIsHidden() => Assert.IsFalse(_style.TryGet(AwarenessLevel.None, out _, out _));

        [Test]
        public void Indicator_FollowsSourceChanges()
        {
            var indicator = CreateIndicator();
            var source = new FakeSource { Awareness = AwarenessLevel.Sleeping, IndicatorAnchor = _host.transform };

            Assert.IsTrue(indicator.Bind(source));
            var label = _host.GetComponent<TextMeshPro>();
            Assert.AreEqual("Zzz", label.text);

            source.Raise(AwarenessLevel.Alerted);
            Assert.AreEqual("!", label.text);

            source.Raise(AwarenessLevel.None);
            Assert.IsFalse(label.enabled);
        }

        [Test]
        public void Resolver_ComponentWithoutInterface_LogsAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[Awareness\]"));

            Assert.IsNull(AwarenessSourceResolver.Resolve(CreateIndicator(), _host));
        }

        [Test]
        public void Resolver_Null_LogsAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[Awareness\]"));

            Assert.IsNull(AwarenessSourceResolver.Resolve(null, _host));
        }

        private AwarenessIndicatorUI CreateIndicator()
        {
            var indicator = _host.AddComponent<AwarenessIndicatorUI>();
            var serialized = new UnityEditor.SerializedObject(indicator);
            serialized.FindProperty("_style").objectReferenceValue = _style;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return indicator;
        }
    }
}
