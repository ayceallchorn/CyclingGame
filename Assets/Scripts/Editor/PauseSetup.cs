using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cycling.Editor
{
    public static class PauseSetup
    {
        [MenuItem("Cycling/Add Pause UI")]
        public static void AddPauseUI()
        {
            var hudCanvas = GameObject.Find("HUD Canvas");
            if (hudCanvas == null)
            {
                Debug.LogError("HUD Canvas not found.");
                return;
            }

            // Fullscreen dim overlay
            var panelGO = new GameObject("Pause Panel", typeof(RectTransform));
            panelGO.transform.SetParent(hudCanvas.transform, false);
            var rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = panelGO.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.6f);

            // PAUSED text
            var textGO = new GameObject("Pause Text", typeof(RectTransform));
            textGO.transform.SetParent(panelGO.transform, false);
            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(600, 120);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "PAUSED";
            tmp.fontSize = 80;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Subtitle
            var subGO = new GameObject("Pause Sub", typeof(RectTransform));
            subGO.transform.SetParent(panelGO.transform, false);
            var srt = subGO.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0, -60);
            srt.sizeDelta = new Vector2(400, 40);
            var subTmp = subGO.AddComponent<TextMeshProUGUI>();
            subTmp.text = "Press P to resume";
            subTmp.fontSize = 24;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.7f, 0.7f, 0.7f);

            // Add PauseUI component
            var pauseUI = hudCanvas.AddComponent<UI.PauseUI>();
            var so = new SerializedObject(pauseUI);
            so.FindProperty("pausePanel").objectReferenceValue = panelGO;
            so.FindProperty("pauseText").objectReferenceValue = tmp;
            so.ApplyModifiedProperties();

            Debug.Log("Pause UI added.");
        }
    }
}
