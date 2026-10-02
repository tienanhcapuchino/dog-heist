using DogHeist.Gameplay.Match;
using DogHeist.Gameplay.Thief;
using DogHeist.UI.Hud;
using DogHeist.UI.Screens;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>
    /// HUD tối thiểu (bước 18 trong README): thanh tiếng ồn, chữ trạng thái, màn kết quả có nút Chơi lại.
    /// </summary>
    internal static class GreyboxHudFactory
    {
        private const string BuiltinUiSprite = "UI/Skin/UISprite.psd";
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 BottomLeft = new Vector2(0f, 0f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        public static GreyboxHud Create(Transform parent, ThiefMotor thief, MatchManager match)
        {
            var canvasObject = GreyboxPrimitives.CreateEmpty("HUD Canvas", parent, Vector3.zero);
            canvasObject.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var hud = new GreyboxHud
            {
                Canvas = canvas,
                NoiseMeter = CreateNoiseMeter(canvas.transform, thief),
                Status = CreateStatus(canvasObject, thief),
                ResultScreen = CreateResultScreen(canvasObject, match)
            };

            var eventSystem = GreyboxPrimitives.CreateEmpty("EventSystem", parent, Vector3.zero);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            return hud;
        }

        private static NoiseMeterUI CreateNoiseMeter(Transform canvas, ThiefMotor thief)
        {
            var background = CreateImage("NoiseMeter", canvas, BottomLeft, new Vector2(40f, 40f),
                new Vector2(360f, 28f), new Color(0f, 0f, 0f, 0.6f));

            var fill = CreateImage("Fill", background.transform, BottomLeft, Vector2.zero, Vector2.zero, Color.white);
            Stretch(fill.rectTransform);
            // Image Type = Filled chỉ hoạt động khi có sprite, nên dùng sprite có sẵn của Unity.
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinUiSprite);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            var meter = background.gameObject.AddComponent<NoiseMeterUI>();
            SerializedWiring.Assign(meter, "_thief", thief);
            SerializedWiring.Assign(meter, "_fill", fill);
            return meter;
        }

        private static ThiefStatusUI CreateStatus(GameObject canvasObject, ThiefMotor thief)
        {
            var canvas = canvasObject.transform;
            var prompt = CreateText("PromptText", canvas, BottomCenter, new Vector2(0f, 120f), new Vector2(1000f, 50f), 32f, TextAlignmentOptions.Center);
            var lure = CreateText("LureText", canvas, TopLeft, new Vector2(40f, -40f), new Vector2(600f, 40f), 28f, TextAlignmentOptions.Left);
            var visibilityText = CreateText("VisibilityText", canvas, TopLeft, new Vector2(40f, -90f), new Vector2(600f, 40f), 28f, TextAlignmentOptions.Left);

            var status = canvasObject.AddComponent<ThiefStatusUI>();
            SerializedWiring.Assign(status, "_interactor", thief.GetComponent<ThiefInteractor>());
            SerializedWiring.Assign(status, "_visibility", thief.GetComponent<ThiefVisibility>());
            SerializedWiring.Assign(status, "_promptText", prompt);
            SerializedWiring.Assign(status, "_lureText", lure);
            SerializedWiring.Assign(status, "_visibilityText", visibilityText);
            return status;
        }

        // ResultScreenUI gắn lên Canvas (luôn bật), không gắn lên panel bị ẩn, để còn nhận sự kiện MatchEnded.
        private static ResultScreenUI CreateResultScreen(GameObject canvasObject, MatchManager match)
        {
            var panel = CreateImage("ResultPanel", canvasObject.transform, Center, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.75f));
            Stretch(panel.rectTransform);

            var title = CreateText("Title", panel.transform, TopCenter, new Vector2(0f, -160f), new Vector2(1400f, 90f), 64f, TextAlignmentOptions.Center);
            var details = CreateText("Details", panel.transform, Center, Vector2.zero, new Vector2(900f, 360f), 32f, TextAlignmentOptions.Center);
            var button = CreateButton("RestartButton", panel.transform, BottomCenter, new Vector2(0f, 160f), new Vector2(320f, 80f), "Chơi lại");
            panel.gameObject.SetActive(false);

            var resultScreen = canvasObject.AddComponent<ResultScreenUI>();
            SerializedWiring.Assign(resultScreen, "_panel", panel.gameObject);
            SerializedWiring.Assign(resultScreen, "_titleText", title);
            SerializedWiring.Assign(resultScreen, "_detailsText", details);
            SerializedWiring.Assign(resultScreen, "_restartButton", button);
            SerializedWiring.Assign(resultScreen, "_matchManager", match);
            return resultScreen;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = parent.gameObject.layer;
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var image = CreateRect(name, parent, anchor, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            float fontSize, TextAlignmentOptions alignment)
        {
            var text = CreateRect(name, parent, anchor, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = string.Empty;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, string label)
        {
            var image = CreateImage(name, parent, anchor, position, size, new Color(0.95f, 0.75f, 0.3f));
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText("Label", image.transform, Center, Vector2.zero, Vector2.zero, 32f, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            text.color = Color.black;
            text.text = label;
            return button;
        }
    }
}
