using Unity.Cinemachine;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Main Camera có CinemachineBrain, cộng một CinemachineCamera quay quanh trộm (chuột hoặc cần phải để xoay).
    /// Tương đương bước 16 trong README.
    /// </summary>
    internal static class GreyboxCameraFactory
    {
        private const float OrbitRadius = 6f;
        private static readonly Vector3 LookOffset = new Vector3(0f, 1.2f, 0f);
        private static readonly Color NightSky = new Color(0.03f, 0.04f, 0.08f);

        public static Camera Create(Transform parent, Transform target)
        {
            var startPosition = target.position + new Vector3(-OrbitRadius, 3f, 0f);

            var mainCameraObject = GreyboxPrimitives.CreateEmpty("Main Camera", parent, startPosition);
            mainCameraObject.tag = "MainCamera";
            var camera = mainCameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = NightSky;
            mainCameraObject.AddComponent<AudioListener>();
            mainCameraObject.AddComponent<CinemachineBrain>();

            var freeLookObject = GreyboxPrimitives.CreateEmpty("FreeLook Camera", parent, startPosition);
            var freeLook = freeLookObject.AddComponent<CinemachineCamera>();
            freeLook.Follow = target;
            var orbit = freeLookObject.AddComponent<CinemachineOrbitalFollow>();
            orbit.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbit.Radius = OrbitRadius;
            var composer = freeLookObject.AddComponent<CinemachineRotationComposer>();
            composer.TargetOffset = LookOffset;
            freeLookObject.AddComponent<CinemachineInputAxisController>();

            return camera;
        }
    }
}
