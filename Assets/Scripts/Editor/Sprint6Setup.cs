using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace Cycling.Editor
{
    public static class Sprint6Setup
    {
        [MenuItem("Cycling/Create Track Definition")]
        public static void CreateTrackDefinition()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data/Tracks"))
                AssetDatabase.CreateFolder("Assets/Data", "Tracks");

            var track = ScriptableObject.CreateInstance<Data.TrackDefinition>();
            track.trackName = "Circuit One";
            track.sceneName = "RaceScene";
            track.length = 182f;
            AssetDatabase.CreateAsset(track, "Assets/Data/Tracks/CircuitOne.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Created CircuitOne.asset");
        }

        [MenuItem("Cycling/Add Countdown + Results to RaceScene")]
        public static void AddCountdownAndResults()
        {
            // --- Countdown text (big centered) ---
            var hudCanvas = GameObject.Find("HUD Canvas");
            if (hudCanvas != null)
            {
                var countdownGO = new GameObject("Countdown Text", typeof(RectTransform));
                countdownGO.transform.SetParent(hudCanvas.transform, false);
                var rt = countdownGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, 50);
                rt.sizeDelta = new Vector2(400, 200);

                var tmp = countdownGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "3";
                tmp.fontSize = 120;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.enableWordWrapping = false;

                // Wire to RaceHUD
                var hud = hudCanvas.GetComponent<UI.RaceHUD>();
                if (hud != null)
                {
                    var so = new SerializedObject(hud);
                    so.FindProperty("countdownText").objectReferenceValue = tmp;
                    so.ApplyModifiedProperties();
                }
            }

            // --- Results UI ---
            var resultsCanvasGO = new GameObject("Results Canvas");
            var resultsCanvas = resultsCanvasGO.AddComponent<Canvas>();
            resultsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            resultsCanvas.sortingOrder = 20;
            var scaler = resultsCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            resultsCanvasGO.AddComponent<GraphicRaycaster>();

            // Results panel (centered)
            var panelGO = new GameObject("Results Panel", typeof(RectTransform));
            panelGO.transform.SetParent(resultsCanvasGO.transform, false);
            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0.1f);
            panelRT.anchorMax = new Vector2(0.8f, 0.9f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.85f);

            // Title
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(panelGO.transform, false);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -20);
            titleRT.sizeDelta = new Vector2(0, 60);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "RACE FINISHED";
            titleTMP.fontSize = 48;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.yellow;

            // Results text (scrollable area)
            var resultsTextGO = new GameObject("Results Text", typeof(RectTransform));
            resultsTextGO.transform.SetParent(panelGO.transform, false);
            var resultsRT = resultsTextGO.GetComponent<RectTransform>();
            resultsRT.anchorMin = new Vector2(0.05f, 0.15f);
            resultsRT.anchorMax = new Vector2(0.95f, 0.85f);
            resultsRT.offsetMin = Vector2.zero;
            resultsRT.offsetMax = Vector2.zero;
            var resultsTMP = resultsTextGO.AddComponent<TextMeshProUGUI>();
            resultsTMP.text = "";
            resultsTMP.fontSize = 24;
            resultsTMP.alignment = TextAlignmentOptions.TopLeft;
            resultsTMP.color = Color.white;

            // Back to Menu button
            var btnGO = new GameObject("Menu Button", typeof(RectTransform));
            btnGO.transform.SetParent(panelGO.transform, false);
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.3f, 0);
            btnRT.anchorMax = new Vector2(0.7f, 0);
            btnRT.pivot = new Vector2(0.5f, 0);
            btnRT.anchoredPosition = new Vector2(0, 20);
            btnRT.sizeDelta = new Vector2(0, 50);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 1f, 1f);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnTextGO = new GameObject("Text", typeof(RectTransform));
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnTextRT = btnTextGO.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;
            var btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "BACK TO MENU";
            btnTMP.fontSize = 24;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.color = Color.white;

            // Add RaceResultsUI component
            var resultsUI = resultsCanvasGO.AddComponent<UI.RaceResultsUI>();
            var rso = new SerializedObject(resultsUI);
            rso.FindProperty("resultsPanel").objectReferenceValue = panelGO;
            rso.FindProperty("titleText").objectReferenceValue = titleTMP;
            rso.FindProperty("resultsText").objectReferenceValue = resultsTMP;
            rso.FindProperty("menuButton").objectReferenceValue = btn;
            rso.ApplyModifiedProperties();

            Debug.Log("Countdown text + Results UI added to RaceScene.");
        }

        [MenuItem("Cycling/Create MainMenu Scene")]
        public static void CreateMainMenuScene()
        {
            // Save current scene first
            EditorSceneManager.SaveOpenScenes();

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();

            // GameManager
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<Core.GameManager>();

            // Load track definition
            var circuitOne = AssetDatabase.LoadAssetAtPath<Data.TrackDefinition>("Assets/Data/Tracks/CircuitOne.asset");
            if (circuitOne != null)
            {
                var gmSO = new SerializedObject(gm);
                var tracksProp = gmSO.FindProperty("tracks");
                tracksProp.arraySize = 1;
                tracksProp.GetArrayElementAtIndex(0).objectReferenceValue = circuitOne;
                gmSO.FindProperty("selectedTrack").objectReferenceValue = circuitOne;
                gmSO.ApplyModifiedProperties();
            }

            // Canvas
            var canvasGO = new GameObject("Menu Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Background panel
            var bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);

            // Title
            var titleGO = CreateTMP(canvasGO.transform, "Title", "CYCLING",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(600, 80), 64, Color.white);

            // Center panel for settings
            var settingsPanel = new GameObject("Settings Panel", typeof(RectTransform));
            settingsPanel.transform.SetParent(canvasGO.transform, false);
            var spRT = settingsPanel.GetComponent<RectTransform>();
            spRT.anchorMin = new Vector2(0.25f, 0.2f);
            spRT.anchorMax = new Vector2(0.75f, 0.8f);
            spRT.offsetMin = Vector2.zero;
            spRT.offsetMax = Vector2.zero;
            var spImg = settingsPanel.AddComponent<Image>();
            spImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            float y = -30f;

            // Track dropdown
            var trackInfoLabel = CreateTMP(settingsPanel.transform, "Track Info", "Circuit One — 182m",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, y), new Vector2(0, 35), 22, Color.white);
            y -= 50f;

            var trackDropdown = CreateDropdown(settingsPanel.transform, "Track Dropdown",
                new Vector2(0, y), new Vector2(0, 35));
            y -= 60f;

            // Laps
            var lapsLabel = CreateTMP(settingsPanel.transform, "Laps Label", "Laps: 3",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, y), new Vector2(0, 30), 20, Color.white);
            y -= 35f;
            var lapsSlider = CreateMenuSlider(settingsPanel.transform, "Laps Slider", new Vector2(0, y));
            y -= 55f;

            // AI Count
            var aiLabel = CreateTMP(settingsPanel.transform, "AI Count Label", "AI Riders: 20",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, y), new Vector2(0, 30), 20, Color.white);
            y -= 35f;
            var aiSlider = CreateMenuSlider(settingsPanel.transform, "AI Count Slider", new Vector2(0, y));
            y -= 55f;

            // Difficulty
            var diffLabel = CreateTMP(settingsPanel.transform, "Difficulty Label", "Difficulty: 50%",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, y), new Vector2(0, 30), 20, Color.white);
            y -= 35f;
            var diffSlider = CreateMenuSlider(settingsPanel.transform, "Difficulty Slider", new Vector2(0, y));
            y -= 55f;

            // Start button
            var startBtnGO = new GameObject("Start Button", typeof(RectTransform));
            startBtnGO.transform.SetParent(settingsPanel.transform, false);
            var sbRT = startBtnGO.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(0.2f, 0);
            sbRT.anchorMax = new Vector2(0.8f, 0);
            sbRT.pivot = new Vector2(0.5f, 0);
            sbRT.anchoredPosition = new Vector2(0, 20);
            sbRT.sizeDelta = new Vector2(0, 60);
            var sbImg = startBtnGO.AddComponent<Image>();
            sbImg.color = new Color(0.1f, 0.7f, 0.2f, 1f);
            var startBtn = startBtnGO.AddComponent<Button>();
            startBtn.targetGraphic = sbImg;

            var startTextGO = new GameObject("Text", typeof(RectTransform));
            startTextGO.transform.SetParent(startBtnGO.transform, false);
            var stRT = startTextGO.GetComponent<RectTransform>();
            stRT.anchorMin = Vector2.zero;
            stRT.anchorMax = Vector2.one;
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            var startTMP = startTextGO.AddComponent<TextMeshProUGUI>();
            startTMP.text = "START RACE";
            startTMP.fontSize = 32;
            startTMP.alignment = TextAlignmentOptions.Center;
            startTMP.color = Color.white;

            // Add MainMenuUI and wire
            var menuUI = canvasGO.AddComponent<UI.MainMenuUI>();
            var muiSO = new SerializedObject(menuUI);
            muiSO.FindProperty("trackDropdown").objectReferenceValue = trackDropdown;
            muiSO.FindProperty("lapsSlider").objectReferenceValue = lapsSlider;
            muiSO.FindProperty("aiCountSlider").objectReferenceValue = aiSlider;
            muiSO.FindProperty("difficultySlider").objectReferenceValue = diffSlider;
            muiSO.FindProperty("lapsLabel").objectReferenceValue = lapsLabel.GetComponent<TextMeshProUGUI>();
            muiSO.FindProperty("aiCountLabel").objectReferenceValue = aiLabel.GetComponent<TextMeshProUGUI>();
            muiSO.FindProperty("difficultyLabel").objectReferenceValue = diffLabel.GetComponent<TextMeshProUGUI>();
            muiSO.FindProperty("trackInfoLabel").objectReferenceValue = trackInfoLabel.GetComponent<TextMeshProUGUI>();
            muiSO.FindProperty("startButton").objectReferenceValue = startBtn;
            muiSO.ApplyModifiedProperties();

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("MainMenu scene created and saved.");
        }

        [MenuItem("Cycling/Setup Build Settings")]
        public static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/RaceScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("Build settings updated: MainMenu (0), RaceScene (1).");
        }

        static GameObject CreateTMP(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, float fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;

            return go;
        }

        static Slider CreateMenuSlider(Transform parent, string name, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 1);
            rt.anchorMax = new Vector2(0.9f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(0, 20);

            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.3f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.sizeDelta = Vector2.zero;
            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0.5f, 1); fillRt.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.6f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one; haRt.sizeDelta = new Vector2(-20, 0);
            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(20, 0); hRt.anchorMin = Vector2.zero; hRt.anchorMax = new Vector2(0, 1);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handleImg;
            return slider;
        }

        static TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 1);
            rt.anchorMax = new Vector2(0.9f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(0, size.y);

            var bgImg = go.AddComponent<Image>();
            bgImg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            // Label
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10, 0); lrt.offsetMax = new Vector2(-25, 0);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Circuit One";
            labelTMP.fontSize = 18;
            labelTMP.alignment = TextAlignmentOptions.Left;
            labelTMP.color = Color.white;

            // Arrow
            var arrowGO = new GameObject("Arrow", typeof(RectTransform));
            arrowGO.transform.SetParent(go.transform, false);
            var art = arrowGO.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(1, 0); art.anchorMax = new Vector2(1, 1);
            art.pivot = new Vector2(1, 0.5f);
            art.sizeDelta = new Vector2(25, 0);
            var arrowTMP = arrowGO.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "v";
            arrowTMP.fontSize = 18;
            arrowTMP.alignment = TextAlignmentOptions.Center;
            arrowTMP.color = Color.white;

            // Template (hidden)
            var templateGO = new GameObject("Template", typeof(RectTransform));
            templateGO.transform.SetParent(go.transform, false);
            var trt = templateGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0); trt.anchorMax = new Vector2(1, 0);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = Vector2.zero;
            trt.sizeDelta = new Vector2(0, 100);
            var tImg = templateGO.AddComponent<Image>();
            tImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var scrollRect = templateGO.AddComponent<ScrollRect>();

            // Viewport
            var viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(templateGO.transform, false);
            var vrt = viewportGO.GetComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
            viewportGO.AddComponent<Image>().color = Color.clear;
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var crt = contentGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 28);

            scrollRect.content = crt;
            scrollRect.viewport = vrt;

            // Item
            var itemGO = new GameObject("Item", typeof(RectTransform));
            itemGO.transform.SetParent(contentGO.transform, false);
            var irt = itemGO.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(1, 0.5f);
            irt.sizeDelta = new Vector2(0, 28);
            var itemToggle = itemGO.AddComponent<Toggle>();

            // Item background
            var itemBgGO = new GameObject("Item Background", typeof(RectTransform));
            itemBgGO.transform.SetParent(itemGO.transform, false);
            var ibrt = itemBgGO.GetComponent<RectTransform>();
            ibrt.anchorMin = Vector2.zero; ibrt.anchorMax = Vector2.one;
            ibrt.offsetMin = Vector2.zero; ibrt.offsetMax = Vector2.zero;
            var ibImg = itemBgGO.AddComponent<Image>();
            ibImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            // Item checkmark (hidden)
            var checkGO = new GameObject("Item Checkmark", typeof(RectTransform));
            checkGO.transform.SetParent(itemBgGO.transform, false);
            var chrt = checkGO.GetComponent<RectTransform>();
            chrt.anchorMin = Vector2.zero; chrt.anchorMax = new Vector2(0, 1);
            chrt.sizeDelta = new Vector2(20, 0);
            var checkImg = checkGO.AddComponent<Image>();
            checkImg.color = new Color(0.2f, 0.6f, 1f);

            itemToggle.targetGraphic = ibImg;
            itemToggle.graphic = checkImg;

            // Item label
            var itemLabelGO = new GameObject("Item Label", typeof(RectTransform));
            itemLabelGO.transform.SetParent(itemGO.transform, false);
            var ilrt = itemLabelGO.GetComponent<RectTransform>();
            ilrt.anchorMin = Vector2.zero; ilrt.anchorMax = Vector2.one;
            ilrt.offsetMin = new Vector2(25, 0); ilrt.offsetMax = Vector2.zero;
            var itemLabelTMP = itemLabelGO.AddComponent<TextMeshProUGUI>();
            itemLabelTMP.text = "Option";
            itemLabelTMP.fontSize = 16;
            itemLabelTMP.alignment = TextAlignmentOptions.Left;
            itemLabelTMP.color = Color.white;

            templateGO.SetActive(false);

            // Create TMP_Dropdown
            var dropdown = go.AddComponent<TMP_Dropdown>();
            dropdown.template = trt;
            dropdown.captionText = labelTMP;
            dropdown.itemText = itemLabelTMP;
            dropdown.targetGraphic = bgImg;

            return dropdown;
        }
    }
}
