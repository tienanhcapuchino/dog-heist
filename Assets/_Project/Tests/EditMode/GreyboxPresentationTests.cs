using System.Linq;
using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxPresentationTests : GreyboxSceneTestBase
    {
        private EditorBuildSettingsScene[] _originalBuildScenes;

        [SetUp]
        public void RememberBuildScenes() => _originalBuildScenes = EditorBuildSettings.scenes;

        [TearDown]
        public void RestoreBuildScenes() => EditorBuildSettings.scenes = _originalBuildScenes;

        [Test]
        public void BuildInto_CreatesSingleMainCameraFollowingThief()
        {
            var result = Build();

            Assert.AreEqual(1, result.Root.GetComponentsInChildren<Camera>().Count(c => c.CompareTag("MainCamera")));
            Assert.IsNotNull(result.MainCamera.GetComponent<CinemachineBrain>());
            var follow = result.Root.GetComponentInChildren<CinemachineCamera>();
            Assert.AreSame(result.Thief.transform, follow.Follow);
            Assert.AreSame(result.MainCamera.transform, ReadReference(result.Thief, "_cameraTransform"));
        }

        [Test]
        public void BuildInto_WiresHud()
        {
            var result = Build();
            var hud = result.Hud;

            Assert.AreSame(result.Thief, ReadReference(hud.NoiseMeter, "_thief"));
            Assert.IsNotNull(ReadReference(hud.NoiseMeter, "_fill"));
            Assert.IsNotNull(ReadReference(hud.Status, "_interactor"));
            Assert.IsNotNull(ReadReference(hud.Status, "_promptText"));
            Assert.AreSame(result.Match, ReadReference(hud.ResultScreen, "_matchManager"));
            Assert.IsNotNull(ReadReference(hud.ResultScreen, "_panel"));
            Assert.IsNotNull(ReadReference(hud.ResultScreen, "_restartButton"));
            Assert.AreNotSame(hud.ResultScreen.gameObject, ReadReference(hud.ResultScreen, "_panel"),
                "ResultScreenUI phải gắn lên Canvas, không gắn lên chính panel");
            Assert.IsNotNull(result.Root.GetComponentInChildren<EventSystem>());
        }

        [Test]
        public void EnsureSceneInBuild_CalledTwice_AddsSceneOnce()
        {
            GreyboxBuildSettings.EnsureSceneInBuild(GreyboxLevelBuilder.ScenePath);
            GreyboxBuildSettings.EnsureSceneInBuild(GreyboxLevelBuilder.ScenePath);

            var matches = EditorBuildSettings.scenes.Where(s => s.path == GreyboxLevelBuilder.ScenePath).ToArray();
            Assert.AreEqual(1, matches.Length);
            Assert.IsTrue(matches[0].enabled);
        }

        [Test]
        public void FindMissingPrerequisites_ProjectIsSetUp_ReturnsNothing()
        {
            // Test này đồng thời kiểm tra Task 0 đã làm đủ (URP, TMP Essentials).
            CollectionAssert.IsEmpty(GreyboxLevelBuilder.FindMissingPrerequisites());
        }
    }
}
