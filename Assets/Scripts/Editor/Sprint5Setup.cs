using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cycling.Editor
{
    public static class Sprint5Setup
    {
        [MenuItem("Cycling/Add Difficulty Slider")]
        public static void AddDifficultySlider()
        {
            var debugCanvas = GameObject.Find("Debug Canvas");
            if (debugCanvas == null)
            {
                Debug.LogError("Debug Canvas not found.");
                return;
            }

            var debugPanel = debugCanvas.transform.Find("Debug Panel");
            if (debugPanel == null)
            {
                Debug.LogError("Debug Panel not found.");
                return;
            }

            // Expand panel height
            var panelRT = debugPanel.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(350, 375);

            float yPos = -270f;

            // Create difficulty label
            var labelGO = new GameObject("Difficulty Label", typeof(RectTransform));
            labelGO.transform.SetParent(debugPanel, false);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.pivot = new Vector2(0, 1);
            labelRT.anchoredPosition = new Vector2(15, yPos);
            labelRT.sizeDelta = new Vector2(0, 26);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "Difficulty: 50%";
            labelTMP.fontSize = 16;
            labelTMP.alignment = TextAlignmentOptions.TopLeft;
            labelTMP.color = Color.white;

            // Create difficulty slider
            var sliderGO = new GameObject("Difficulty Slider", typeof(RectTransform));
            sliderGO.transform.SetParent(debugPanel, false);
            var sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.05f, 1);
            sliderRT.anchorMax = new Vector2(0.95f, 1);
            sliderRT.pivot = new Vector2(0.5f, 1);
            sliderRT.anchoredPosition = new Vector2(0, yPos - 30);
            sliderRT.sizeDelta = new Vector2(0, 20);

            // Background
            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(sliderGO.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGO.transform, false);
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
            fillImg.color = new Color(1f, 0.5f, 0.1f, 1f); // orange for difficulty

            // Handle area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGO.transform, false);
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

            var slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            // Wire into DebugPanel
            var debugPanelComp = debugCanvas.GetComponent<UI.DebugPanel>();
            if (debugPanelComp != null)
            {
                var so = new SerializedObject(debugPanelComp);
                so.FindProperty("difficultySlider").objectReferenceValue = slider;
                so.FindProperty("difficultyLabel").objectReferenceValue = labelTMP;
                so.ApplyModifiedProperties();
            }

            Debug.Log("Difficulty slider added to debug panel.");
        }
    }
}
