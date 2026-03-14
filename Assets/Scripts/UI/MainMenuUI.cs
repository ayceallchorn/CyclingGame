using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cycling.Core;
using Cycling.Data;

namespace Cycling.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] TMP_Dropdown trackDropdown;
        [SerializeField] Slider lapsSlider;
        [SerializeField] Slider aiCountSlider;
        [SerializeField] Slider difficultySlider;
        [SerializeField] TextMeshProUGUI lapsLabel;
        [SerializeField] TextMeshProUGUI aiCountLabel;
        [SerializeField] TextMeshProUGUI difficultyLabel;
        [SerializeField] TextMeshProUGUI trackInfoLabel;
        [SerializeField] Button startButton;

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Populate track dropdown
            if (trackDropdown != null && gm.tracks != null)
            {
                trackDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();
                foreach (var t in gm.tracks)
                    options.Add(t != null ? t.trackName : "Unknown");
                trackDropdown.AddOptions(options);
                trackDropdown.onValueChanged.AddListener(OnTrackChanged);
                OnTrackChanged(0);
            }

            // Laps slider
            if (lapsSlider != null)
            {
                lapsSlider.minValue = 1;
                lapsSlider.maxValue = 20;
                lapsSlider.wholeNumbers = true;
                lapsSlider.value = gm.laps;
                lapsSlider.onValueChanged.AddListener(OnLapsChanged);
                OnLapsChanged(gm.laps);
            }

            // AI count slider
            if (aiCountSlider != null)
            {
                aiCountSlider.minValue = 5;
                aiCountSlider.maxValue = 25;
                aiCountSlider.wholeNumbers = true;
                aiCountSlider.value = gm.aiCount;
                aiCountSlider.onValueChanged.AddListener(OnAICountChanged);
                OnAICountChanged(gm.aiCount);
            }

            // Difficulty slider
            if (difficultySlider != null)
            {
                difficultySlider.minValue = 0;
                difficultySlider.maxValue = 100;
                difficultySlider.value = gm.difficulty * 100f;
                difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
                OnDifficultyChanged(gm.difficulty * 100f);
            }

            // Start button
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        void OnTrackChanged(int index)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.tracks == null || index >= gm.tracks.Length) return;
            gm.selectedTrack = gm.tracks[index];
            if (trackInfoLabel != null && gm.selectedTrack != null)
                trackInfoLabel.text = $"{gm.selectedTrack.trackName} — {gm.selectedTrack.length:F0}m";
        }

        void OnLapsChanged(float value)
        {
            int laps = Mathf.RoundToInt(value);
            if (GameManager.Instance != null) GameManager.Instance.laps = laps;
            if (lapsLabel != null) lapsLabel.text = $"Laps: {laps}";
        }

        void OnAICountChanged(float value)
        {
            int count = Mathf.RoundToInt(value);
            if (GameManager.Instance != null) GameManager.Instance.aiCount = count;
            if (aiCountLabel != null) aiCountLabel.text = $"AI Riders: {count}";
        }

        void OnDifficultyChanged(float value)
        {
            if (GameManager.Instance != null) GameManager.Instance.difficulty = value / 100f;
            if (difficultyLabel != null) difficultyLabel.text = $"Difficulty: {value:F0}%";
        }

        void OnStartClicked()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                Debug.Log($"[MainMenu] Starting race: laps={gm.laps}, AI={gm.aiCount}, difficulty={gm.difficulty:P0}");
                gm.StartRace();
            }
            else
            {
                Debug.LogError("[MainMenu] No GameManager found!");
            }
        }
    }
}
