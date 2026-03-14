using UnityEngine;
using TMPro;
using Cycling.Cycling;
using Cycling.Race;

namespace Cycling.UI
{
    public class RaceHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] RiderMotor riderMotor;
        [SerializeField] GearSystem gearSystem;
        [SerializeField] PositionTracker positionTracker;

        [Header("HUD Text Elements")]
        [SerializeField] TextMeshProUGUI powerText;
        [SerializeField] TextMeshProUGUI speedText;
        [SerializeField] TextMeshProUGUI cadenceText;
        [SerializeField] TextMeshProUGUI hrText;
        [SerializeField] TextMeshProUGUI gearText;
        [SerializeField] TextMeshProUGUI gradientText;
        [SerializeField] TextMeshProUGUI positionText;
        [SerializeField] TextMeshProUGUI lapText;
        [SerializeField] TextMeshProUGUI draftText;

        [Header("Countdown")]
        [SerializeField] TextMeshProUGUI countdownText;

        RaceManager _raceManager;

        void Start()
        {
            _raceManager = Object.FindFirstObjectByType<RaceManager>();
        }

        void Update()
        {
            UpdateCountdown();

            if (riderMotor == null) return;

            if (powerText != null)
                powerText.text = $"{riderMotor.PowerWatts:F0}W";

            if (speedText != null)
                speedText.text = $"{riderMotor.SpeedKmh:F1} km/h";

            if (gradientText != null)
            {
                float grad = riderMotor.Gradient * 100f;
                gradientText.text = $"{grad:F1}%";
            }

            if (gearSystem != null && gearText != null)
                gearText.text = $"{gearSystem.CurrentGearIndex}/{gearSystem.TotalGears}";

            if (positionTracker != null && positionText != null)
            {
                int pos = positionTracker.GetPosition(riderMotor);
                positionText.text = pos > 0 ? $"P{pos}/{positionTracker.TotalRiders}" : "--";
            }

            var lapTracker = riderMotor.GetComponent<LapTracker>();
            if (lapTracker != null && lapText != null)
                lapText.text = $"Lap {lapTracker.CurrentLap + 1}/{lapTracker.TotalLaps}";

            if (draftText != null)
            {
                float draft = riderMotor.DraftFactor * 100f;
                draftText.text = draft > 0.5f ? $"Draft {draft:F0}%" : "";
            }

            var debug = DebugPanel.Instance;
            if (debug != null)
            {
                if (cadenceText != null)
                    cadenceText.text = $"{debug.SimulatedCadence:F0} rpm";
                if (hrText != null)
                    hrText.text = $"{debug.SimulatedHR:F0} bpm";
            }
        }

        void UpdateCountdown()
        {
            if (countdownText == null) return;

            if (_raceManager == null || _raceManager.State != RaceState.Countdown)
            {
                countdownText.gameObject.SetActive(false);
                return;
            }

            countdownText.gameObject.SetActive(true);
            float t = _raceManager.CountdownTimer;

            if (t > 3f) countdownText.text = "3";
            else if (t > 2f) countdownText.text = "3";
            else if (t > 1f) countdownText.text = "2";
            else if (t > 0f) countdownText.text = "1";
            else countdownText.text = "GO!";
        }
    }
}
