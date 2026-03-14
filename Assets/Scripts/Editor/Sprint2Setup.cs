using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cycling.Editor
{
    public static class Sprint2Setup
    {
        [MenuItem("Cycling/Create Default Gear Table")]
        public static void CreateGearTable()
        {
            var asset = ScriptableObject.CreateInstance<Data.GearTableData>();

            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");

            AssetDatabase.CreateAsset(asset, "Assets/Data/GearTable_Default.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Created GearTable_Default.asset");
        }

        [MenuItem("Cycling/Setup Race UI")]
        public static void SetupRaceUI()
        {
            // === HUD Canvas ===
            var hudCanvasGO = new GameObject("HUD Canvas");
            var hudCanvas = hudCanvasGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 0;
            var hudScaler = hudCanvasGO.AddComponent<CanvasScaler>();
            hudScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            hudScaler.referenceResolution = new Vector2(1920, 1080);
            hudCanvasGO.AddComponent<GraphicRaycaster>();

            // HUD panel (bottom-left)
            var hudPanel = CreatePanel(hudCanvasGO.transform, "HUD Panel",
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(320, 220),
                new Color(0, 0, 0, 0.5f));
            hudPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 20);

            // HUD text elements
            var raceHUD = hudCanvasGO.AddComponent<UI.RaceHUD>();

            var powerText = CreateText(hudPanel.transform, "Power Text", "200W",
                new Vector2(15, -15), 28, TextAlignmentOptions.TopLeft);
            var speedText = CreateText(hudPanel.transform, "Speed Text", "30.0 km/h",
                new Vector2(15, -50), 28, TextAlignmentOptions.TopLeft);
            var cadenceText = CreateText(hudPanel.transform, "Cadence Text", "90 rpm",
                new Vector2(15, -85), 22, TextAlignmentOptions.TopLeft);
            var hrText = CreateText(hudPanel.transform, "HR Text", "130 bpm",
                new Vector2(15, -115), 22, TextAlignmentOptions.TopLeft);
            var gearText = CreateText(hudPanel.transform, "Gear Text", "11/22",
                new Vector2(15, -145), 22, TextAlignmentOptions.TopLeft);
            var gradientText = CreateText(hudPanel.transform, "Gradient Text", "0.0%",
                new Vector2(15, -175), 22, TextAlignmentOptions.TopLeft);

            // Wire HUD references via SerializedObject
            var hudSO = new SerializedObject(raceHUD);
            hudSO.FindProperty("powerText").objectReferenceValue = powerText;
            hudSO.FindProperty("speedText").objectReferenceValue = speedText;
            hudSO.FindProperty("cadenceText").objectReferenceValue = cadenceText;
            hudSO.FindProperty("hrText").objectReferenceValue = hrText;
            hudSO.FindProperty("gearText").objectReferenceValue = gearText;
            hudSO.FindProperty("gradientText").objectReferenceValue = gradientText;

            // Wire RiderMotor and GearSystem
            var riderMotor = Object.FindFirstObjectByType<Cycling.RiderMotor>();
            var gearSystem = Object.FindFirstObjectByType<Cycling.GearSystem>();
            if (riderMotor != null)
                hudSO.FindProperty("riderMotor").objectReferenceValue = riderMotor;
            if (gearSystem != null)
                hudSO.FindProperty("gearSystem").objectReferenceValue = gearSystem;
            hudSO.ApplyModifiedProperties();

            // === Debug Canvas ===
            var debugCanvasGO = new GameObject("Debug Canvas");
            var debugCanvas = debugCanvasGO.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvas.sortingOrder = 10;
            var debugScaler = debugCanvasGO.AddComponent<CanvasScaler>();
            debugScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            debugScaler.referenceResolution = new Vector2(1920, 1080);
            debugCanvasGO.AddComponent<GraphicRaycaster>();

            // Debug panel (right side)
            var debugPanel = CreatePanel(debugCanvasGO.transform, "Debug Panel",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(350, 300),
                new Color(0.1f, 0.1f, 0.1f, 0.85f));
            debugPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 0);

            var titleText = CreateText(debugPanel.transform, "Title", "DEBUG (F1)",
                new Vector2(15, -10), 20, TextAlignmentOptions.TopLeft);
            titleText.color = Color.yellow;

            // Sliders
            float yPos = -45f;
            var powerLabel = CreateText(debugPanel.transform, "Power Label", "Power: 200W",
                new Vector2(15, yPos), 16, TextAlignmentOptions.TopLeft);
            var powerSlider = CreateSlider(debugPanel.transform, "Power Slider",
                new Vector2(0, yPos - 30), new Vector2(310, 20));

            yPos -= 75f;
            var cadenceLabel = CreateText(debugPanel.transform, "Cadence Label", "Cadence: 90 rpm",
                new Vector2(15, yPos), 16, TextAlignmentOptions.TopLeft);
            var cadenceSlider = CreateSlider(debugPanel.transform, "Cadence Slider",
                new Vector2(0, yPos - 30), new Vector2(310, 20));

            yPos -= 75f;
            var hrLabel = CreateText(debugPanel.transform, "HR Label", "HR: 130 bpm",
                new Vector2(15, yPos), 16, TextAlignmentOptions.TopLeft);
            var hrSlider = CreateSlider(debugPanel.transform, "HR Slider",
                new Vector2(0, yPos - 30), new Vector2(310, 20));

            // Add DebugPanel component and wire
            var debugPanelComp = debugCanvasGO.AddComponent<UI.DebugPanel>();
            var dpSO = new SerializedObject(debugPanelComp);
            dpSO.FindProperty("panel").objectReferenceValue = debugPanel;
            dpSO.FindProperty("powerSlider").objectReferenceValue = powerSlider;
            dpSO.FindProperty("cadenceSlider").objectReferenceValue = cadenceSlider;
            dpSO.FindProperty("hrSlider").objectReferenceValue = hrSlider;
            dpSO.FindProperty("powerLabel").objectReferenceValue = powerLabel;
            dpSO.FindProperty("cadenceLabel").objectReferenceValue = cadenceLabel;
            dpSO.FindProperty("hrLabel").objectReferenceValue = hrLabel;
            if (riderMotor != null)
                dpSO.FindProperty("riderMotor").objectReferenceValue = riderMotor;
            dpSO.ApplyModifiedProperties();

            Debug.Log("Race UI created: HUD Canvas + Debug Canvas");
        }

        static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = color;

            return go;
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            Vector2 position, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(0, fontSize + 10);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            return tmp;
        }

        static Slider CreateSlider(Transform parent, string name,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 1);
            rt.anchorMax = new Vector2(0.95f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(0, size.y);

            // Background
            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.5f, 1);
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.6f, 1f, 1f);

            // Handle slide area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = new Vector2(-20, 0);

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(20, 0);
            hRt.anchorMin = new Vector2(0, 0);
            hRt.anchorMax = new Vector2(0, 1);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }
    }
}
