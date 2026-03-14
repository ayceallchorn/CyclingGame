using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cycling.Core;
using Cycling.Cycling;
using Cycling.Race;

namespace Cycling.UI
{
    public class RaceResultsUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] GameObject resultsPanel;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI resultsText;
        [SerializeField] Button menuButton;

        void Awake()
        {
            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);
        }

        void OnEnable()
        {
            EventBus.OnRaceFinished += ShowResults;
        }

        void OnDisable()
        {
            EventBus.OnRaceFinished -= ShowResults;
        }

        void ShowResults()
        {
            if (resultsPanel != null)
                resultsPanel.SetActive(true);

            if (titleText != null)
                titleText.text = "RACE FINISHED";

            if (resultsText == null) return;

            // Gather all riders sorted by position
            var tracker = Object.FindFirstObjectByType<PositionTracker>();
            var motors = Object.FindObjectsByType<RiderMotor>(FindObjectsSortMode.None);
            var raceManager = Object.FindFirstObjectByType<RaceManager>();

            if (tracker == null || motors.Length == 0)
            {
                resultsText.text = "No results available.";
                return;
            }

            // Sort by position
            var sorted = new List<RiderMotor>(motors);
            sorted.Sort((a, b) =>
            {
                int posA = tracker.GetPosition(a);
                int posB = tracker.GetPosition(b);
                return posA.CompareTo(posB);
            });

            // Build results string
            var sb = new System.Text.StringBuilder();
            float leaderSpeed = sorted.Count > 0 ? sorted[0].SpeedKmh : 0f;

            for (int i = 0; i < sorted.Count && i < 21; i++)
            {
                var rider = sorted[i];
                var identity = rider.GetComponent<RiderIdentity>();
                string name = identity != null ? identity.RiderName : rider.gameObject.name;
                bool isPlayer = identity != null && identity.IsPlayer;

                string prefix = isPlayer ? ">>> " : "    ";
                string suffix = isPlayer ? " <<<" : "";

                if (i == 0)
                    sb.AppendLine($"{prefix}{i + 1}. {name}{suffix}");
                else
                    sb.AppendLine($"{prefix}{i + 1}. {name}{suffix}");
            }

            resultsText.text = sb.ToString();
        }

        void OnMenuClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMenu();
        }
    }
}
