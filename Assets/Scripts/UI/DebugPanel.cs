using UnityEngine;
using UnityEngine.UI;
using Cycling.Cycling;
using Cycling.Input;
using Cycling.Race;
using TMPro;

namespace Cycling.UI
{
    public class DebugPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] RiderMotor riderMotor;
        [SerializeField] RaceManager raceManager;

        [Header("UI Elements")]
        [SerializeField] GameObject panel;
        [SerializeField] Slider powerSlider;
        [SerializeField] Slider cadenceSlider;
        [SerializeField] Slider hrSlider;
        [SerializeField] Slider difficultySlider;
        [SerializeField] TextMeshProUGUI powerLabel;
        [SerializeField] TextMeshProUGUI cadenceLabel;
        [SerializeField] TextMeshProUGUI hrLabel;
        [SerializeField] TextMeshProUGUI difficultyLabel;

        public float SimulatedCadence { get; private set; } = 90f;
        public float SimulatedHR { get; private set; } = 130f;

        public static DebugPanel Instance { get; private set; }

        void Awake()
        {
            Instance = this;

            if (powerSlider != null)
            {
                powerSlider.minValue = 0f;
                powerSlider.maxValue = 500f;
                powerSlider.value = 200f;
                powerSlider.onValueChanged.AddListener(OnPowerChanged);
            }

            if (cadenceSlider != null)
            {
                cadenceSlider.minValue = 0f;
                cadenceSlider.maxValue = 150f;
                cadenceSlider.value = 90f;
                cadenceSlider.onValueChanged.AddListener(OnCadenceChanged);
            }

            if (hrSlider != null)
            {
                hrSlider.minValue = 60f;
                hrSlider.maxValue = 200f;
                hrSlider.value = 130f;
                hrSlider.onValueChanged.AddListener(OnHRChanged);
            }

            if (difficultySlider != null)
            {
                difficultySlider.minValue = 0f;
                difficultySlider.maxValue = 100f;
                difficultySlider.value = 50f;
                difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
            }
        }

        void Start()
        {
            // Auto-find RaceManager if not set
            if (raceManager == null)
                raceManager = Object.FindFirstObjectByType<RaceManager>();

            // Sync difficulty slider from GameManager if available
            var gm = Core.GameManager.Instance;
            if (gm != null && difficultySlider != null)
            {
                difficultySlider.SetValueWithoutNotify(gm.difficulty * 100f);
                if (difficultyLabel != null)
                    difficultyLabel.text = $"Difficulty: {gm.difficulty * 100f:F0}%";
            }

            // Subscribe to input (in Start so InputManager.Instance is ready)
            if (InputManager.Instance != null)
                InputManager.Instance.OnToggleDebug += TogglePanel;
        }

        void OnDestroy()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnToggleDebug -= TogglePanel;
        }

        void TogglePanel()
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }

        void OnPowerChanged(float value)
        {
            if (riderMotor != null)
                riderMotor.PowerWatts = value;
            if (powerLabel != null)
                powerLabel.text = $"Power: {value:F0}W";
        }

        void OnCadenceChanged(float value)
        {
            SimulatedCadence = value;
            if (cadenceLabel != null)
                cadenceLabel.text = $"Cadence: {value:F0} rpm";
        }

        void OnHRChanged(float value)
        {
            SimulatedHR = value;
            if (hrLabel != null)
                hrLabel.text = $"HR: {value:F0} bpm";
        }

        void OnDifficultyChanged(float value)
        {
            if (raceManager != null)
                raceManager.Difficulty = value / 100f;
            if (difficultyLabel != null)
                difficultyLabel.text = $"Difficulty: {value:F0}%";
        }
    }
}
