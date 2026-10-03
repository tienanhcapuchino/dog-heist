using DogHeist.Gameplay.Audio;
using DogHeist.UI.Feedback;
using DogHeist.UI.Hud;
using NUnit.Framework;
using Unity.Cinemachine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxFeedbackTests : GreyboxSceneTestBase
    {
        [Test]
        public void BuildInto_AddsIndicatorsBoundToAi()
        {
            var result = Build();

            Assert.AreSame(result.Dog, ReadReference(result.DogIndicator, "_source"));
            Assert.AreSame(result.Owner, ReadReference(result.OwnerIndicator, "_source"));
            Assert.IsNotNull(ReadReference(result.OwnerIndicator, "_style"));
            Assert.IsNotNull(ReadReference(result.Dog, "_indicatorAnchor"));
        }

        [Test]
        public void BuildInto_AddsSoundPlayers()
        {
            var result = Build();

            Assert.IsNotNull(result.Thief.GetComponent<NoiseSoundPlayer>());
            Assert.IsNotNull(result.Dog.GetComponent<NoiseSoundPlayer>());
            Assert.IsNotNull(result.Dog.GetComponent<AwarenessSoundPlayer>());
            Assert.AreSame(result.Owner, ReadReference(result.Owner.GetComponent<AwarenessSoundPlayer>(), "_source"));
            Assert.IsNotNull(ReadReference(result.Match.GetComponent<MatchStingerPlayer>(), "_library"));
        }

        [Test]
        public void BuildInto_AddsEyeAndCameraShake()
        {
            var result = Build();

            var eye = result.Root.GetComponentInChildren<VisibilityEyeUI>(true);
            Assert.IsNotNull(eye);
            Assert.IsNotNull(ReadReference(eye, "_visibility"));
            var shake = result.Root.GetComponentInChildren<SpottedCameraShake>(true);
            Assert.IsNotNull(ReadReference(shake, "_impulse"));
            Assert.IsNotNull(result.Root.GetComponentInChildren<CinemachineImpulseListener>(true));
        }
    }
}
