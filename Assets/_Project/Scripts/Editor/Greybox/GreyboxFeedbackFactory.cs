using DogHeist.Gameplay.Audio;
using DogHeist.UI.Awareness;
using TMPro;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// Gắn phần phản hồi cho nhân vật greybox: dấu cảnh giác trên đầu AI và các người phát tiếng.
    /// </summary>
    internal static class GreyboxFeedbackFactory
    {
        private const float IndicatorFontSize = 6f;
        private static readonly Vector2 IndicatorSize = new Vector2(4f, 1.5f);

        /// <summary>Tạo điểm đặt dấu trên đầu AI và nối vào ô _indicatorAnchor của nó.</summary>
        public static Transform CreateIndicatorAnchor(Component aiSource, float height)
        {
            var anchor = GreyboxPrimitives.CreateEmpty("IndicatorAnchor", aiSource.transform, new Vector3(0f, height, 0f));
            anchor.layer = aiSource.gameObject.layer;
            SerializedWiring.Assign(aiSource, "_indicatorAnchor", anchor.transform);
            return anchor.transform;
        }

        public static AwarenessIndicatorUI CreateIndicator(Component aiSource, Transform anchor, GreyboxAssets assets)
        {
            var indicatorObject = GreyboxPrimitives.CreateEmpty("AwarenessIndicator", aiSource.transform, anchor.localPosition);
            // Cùng layer Characters với nhân vật, để NavMesh và các luật theo layer coi nó là một phần của nhân vật.
            indicatorObject.layer = aiSource.gameObject.layer;

            // TextMeshPro (bản 3D, không phải bản UI) để chữ nằm trong thế giới game, trên đầu nhân vật.
            var label = indicatorObject.AddComponent<TextMeshPro>();
            label.rectTransform.sizeDelta = IndicatorSize;
            label.fontSize = IndicatorFontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.text = string.Empty;
            if (assets.UiFont != null)
            {
                label.font = assets.UiFont;
            }

            var indicator = indicatorObject.AddComponent<AwarenessIndicatorUI>();
            SerializedWiring.Assign(indicator, "_source", aiSource);
            SerializedWiring.Assign(indicator, "_style", assets.IndicatorStyle);
            return indicator;
        }

        public static void AddVoice(Component aiSource, AwarenessVoice voice, GreyboxAssets assets)
        {
            var gameObject = aiSource.gameObject;
            EnsureAudioSource(gameObject, spatialBlend: 1f);

            var player = gameObject.AddComponent<AwarenessSoundPlayer>();
            SerializedWiring.Assign(player, "_source", aiSource);
            SerializedWiring.Assign(player, "_library", assets.Sounds);
            SetEnum(player, "_voice", (int)voice);
        }

        public static void AddNoiseSound(GameObject emitter, NoiseSoundKind kind, GreyboxAssets assets)
        {
            EnsureAudioSource(emitter, spatialBlend: 1f);

            var player = emitter.AddComponent<NoiseSoundPlayer>();
            SerializedWiring.Assign(player, "_library", assets.Sounds);
            SetEnum(player, "_kind", (int)kind);
        }

        public static void AddMatchStinger(GameObject matchObject, GreyboxAssets assets)
        {
            // Nhạc kết thúc ván là tiếng 2D: nghe như nhau dù camera đang ở đâu.
            EnsureAudioSource(matchObject, spatialBlend: 0f);

            var player = matchObject.AddComponent<MatchStingerPlayer>();
            SerializedWiring.Assign(player, "_library", assets.Sounds);
        }

        // Một nhân vật có thể có nhiều người phát tiếng (chó: sủa và giọng); dùng chung một AudioSource,
        // PlayOneShot cho phép các tiếng chồng lên nhau.
        private static void EnsureAudioSource(GameObject gameObject, float spatialBlend)
        {
            if (gameObject.GetComponent<AudioSource>() != null)
            {
                return;
            }

            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = spatialBlend;
        }

        private static void SetEnum(Object target, string propertyPath, int value)
        {
            var serialized = new UnityEditor.SerializedObject(target);
            serialized.FindProperty(propertyPath).enumValueIndex = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
