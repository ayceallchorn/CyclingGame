using UnityEditor;
using UnityEngine;
using Cycling.Data;
using Cycling.AI;
using TMPro;

namespace Cycling.Editor
{
    public static class Sprint4Setup
    {
        static readonly (string name, Color primary, Color secondary)[] Teams = {
            ("White", new Color(1f, 1f, 1f), new Color(0.2f, 0.2f, 0.2f)),
            ("Yellow", new Color(1f, 0.85f, 0f), new Color(0.1f, 0.1f, 0.1f)),
            ("Sky Blue", new Color(0.4f, 0.75f, 1f), new Color(1f, 1f, 1f)),
            ("Red", new Color(0.9f, 0.15f, 0.15f), new Color(1f, 1f, 1f)),
            ("Green", new Color(0.1f, 0.7f, 0.2f), new Color(1f, 1f, 1f)),
            ("Navy", new Color(0.1f, 0.1f, 0.4f), new Color(1f, 1f, 1f)),
            ("Orange", new Color(1f, 0.5f, 0f), new Color(0.1f, 0.1f, 0.1f)),
            ("Pink", new Color(1f, 0.4f, 0.7f), new Color(0.1f, 0.1f, 0.1f)),
            ("Purple", new Color(0.5f, 0.1f, 0.7f), new Color(1f, 1f, 1f)),
            ("Teal", new Color(0f, 0.6f, 0.6f), new Color(1f, 1f, 1f)),
            ("Black", new Color(0.1f, 0.1f, 0.1f), new Color(1f, 0.85f, 0f)),
            ("Cyan", new Color(0f, 0.9f, 0.9f), new Color(0.1f, 0.1f, 0.1f)),
            ("Maroon", new Color(0.5f, 0f, 0.1f), new Color(1f, 1f, 1f)),
            ("Lime", new Color(0.6f, 0.9f, 0.1f), new Color(0.1f, 0.1f, 0.1f)),
            ("Brown", new Color(0.55f, 0.3f, 0.1f), new Color(1f, 1f, 1f)),
            ("Silver", new Color(0.75f, 0.75f, 0.78f), new Color(0.1f, 0.1f, 0.4f)),
            ("Gold", new Color(0.85f, 0.65f, 0.13f), new Color(0.1f, 0.1f, 0.1f)),
            ("Coral", new Color(1f, 0.4f, 0.3f), new Color(1f, 1f, 1f)),
        };

        static readonly string[] RiderNames = {
            "Alex Martin", "Ben Cooper", "Carlos Vega", "David Park",
            "Erik Holm", "Finn O'Brien", "George Wei", "Hugo Blanc",
            "Ivan Petrov", "Jack Turner", "Karl Berg", "Luca Rossi",
            "Marco Silva", "Nils Strand", "Oscar Rey", "Pavel Novak",
            "Ravi Sharma", "Sam Archer", "Tomas Cruz", "Victor Hale"
        };

        [MenuItem("Cycling/Generate Teams and Riders")]
        public static void GenerateTeamsAndRiders()
        {
            // Create folders
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Teams"))
                AssetDatabase.CreateFolder("Assets/Data", "Teams");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Riders"))
                AssetDatabase.CreateFolder("Assets/Data", "Riders");

            // Create teams
            var teamAssets = new TeamData[Teams.Length];
            for (int i = 0; i < Teams.Length; i++)
            {
                var team = ScriptableObject.CreateInstance<TeamData>();
                team.teamName = Teams[i].name;
                team.primaryColor = Teams[i].primary;
                team.secondaryColor = Teams[i].secondary;
                string path = $"Assets/Data/Teams/Team_{Teams[i].name.Replace(" ", "")}.asset";
                AssetDatabase.CreateAsset(team, path);
                teamAssets[i] = team;
            }

            // Create riders (distribute across teams)
            for (int i = 0; i < RiderNames.Length; i++)
            {
                var rider = ScriptableObject.CreateInstance<RiderData>();
                rider.riderName = RiderNames[i];
                rider.team = teamAssets[i % teamAssets.Length];
                rider.ftp = Random.Range(220f, 340f);
                rider.weight = Random.Range(62f, 82f);
                string safeName = RiderNames[i].Replace(" ", "_").Replace("'", "");
                AssetDatabase.CreateAsset(rider, $"Assets/Data/Riders/Rider_{safeName}.asset");
            }

            // Create default AI strategy
            var strategy = ScriptableObject.CreateInstance<AIStrategyData>();
            strategy.aggressiveness = 0.5f;
            strategy.sprintAbility = 0.5f;
            strategy.climbingAbility = 0.5f;
            strategy.powerVariation = 0.05f;
            AssetDatabase.CreateAsset(strategy, "Assets/Data/AIStrategy_Default.asset");

            // Create default bike
            var bike = ScriptableObject.CreateInstance<BikeData>();
            var gearTable = AssetDatabase.LoadAssetAtPath<GearTableData>("Assets/Data/GearTable_Default.asset");
            bike.gearTable = gearTable;
            AssetDatabase.CreateAsset(bike, "Assets/Data/Bike_Default.asset");

            AssetDatabase.SaveAssets();
            Debug.Log($"Created {teamAssets.Length} teams, {RiderNames.Length} riders, default strategy, default bike.");
        }

        [MenuItem("Cycling/Setup Race Manager")]
        public static void SetupRaceManager()
        {
            // Create Race Manager GO
            var rmGO = new GameObject("Race Manager");
            var rm = rmGO.AddComponent<Race.RaceManager>();

            // Create DraftingSystem
            var draftGO = new GameObject("Drafting System");
            var drafting = draftGO.AddComponent<Cycling.DraftingSystem>();

            // Create PositionTracker
            var posGO = new GameObject("Position Tracker");
            var posTracker = posGO.AddComponent<Race.PositionTracker>();

            // Find track and player
            var track = Object.FindFirstObjectByType<Track.TrackSpline>();
            var playerMotor = GameObject.Find("Rider")?.GetComponent<Cycling.RiderMotor>();

            // Wire DraftingSystem
            var draftSO = new SerializedObject(drafting);
            if (track != null)
                draftSO.FindProperty("trackSpline").objectReferenceValue = track;
            draftSO.ApplyModifiedProperties();

            // Wire PositionTracker
            var posSO = new SerializedObject(posTracker);
            if (track != null)
                posSO.FindProperty("trackSpline").objectReferenceValue = track;
            posSO.ApplyModifiedProperties();

            // Load data assets
            var riderGuids = AssetDatabase.FindAssets("t:RiderData", new[] { "Assets/Data/Riders" });
            var riderDataList = new RiderData[riderGuids.Length];
            for (int i = 0; i < riderGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(riderGuids[i]);
                riderDataList[i] = AssetDatabase.LoadAssetAtPath<RiderData>(path);
            }

            var defaultStrategy = AssetDatabase.LoadAssetAtPath<AIStrategyData>("Assets/Data/AIStrategy_Default.asset");
            var defaultBike = AssetDatabase.LoadAssetAtPath<BikeData>("Assets/Data/Bike_Default.asset");

            // Wire RaceManager
            var rmSO = new SerializedObject(rm);
            if (track != null)
                rmSO.FindProperty("trackSpline").objectReferenceValue = track;
            rmSO.FindProperty("totalLaps").intValue = 3;
            rmSO.FindProperty("aiRiderCount").intValue = 20;
            if (playerMotor != null)
                rmSO.FindProperty("playerMotor").objectReferenceValue = playerMotor;
            rmSO.FindProperty("draftingSystem").objectReferenceValue = drafting;
            rmSO.FindProperty("positionTracker").objectReferenceValue = posTracker;
            if (defaultStrategy != null)
                rmSO.FindProperty("defaultStrategy").objectReferenceValue = defaultStrategy;
            if (defaultBike != null)
                rmSO.FindProperty("defaultBike").objectReferenceValue = defaultBike;

            // Set rider data array
            var riderListProp = rmSO.FindProperty("aiRiderDataList");
            riderListProp.arraySize = riderDataList.Length;
            for (int i = 0; i < riderDataList.Length; i++)
                riderListProp.GetArrayElementAtIndex(i).objectReferenceValue = riderDataList[i];

            rmSO.ApplyModifiedProperties();

            // Wire PositionTracker into RaceHUD
            var raceHUD = Object.FindFirstObjectByType<UI.RaceHUD>();
            if (raceHUD != null)
            {
                var hudSO = new SerializedObject(raceHUD);
                hudSO.FindProperty("positionTracker").objectReferenceValue = posTracker;
                hudSO.ApplyModifiedProperties();
            }

            // Add LapTracker to player
            if (playerMotor != null && playerMotor.GetComponent<Race.LapTracker>() == null)
            {
                var lap = playerMotor.gameObject.AddComponent<Race.LapTracker>();
                var lapSO = new SerializedObject(lap);
                lapSO.FindProperty("totalLaps").intValue = 3;
                lapSO.ApplyModifiedProperties();
            }

            // Add RiderIdentity to player
            if (playerMotor != null && playerMotor.GetComponent<Race.RiderIdentity>() == null)
                playerMotor.gameObject.AddComponent<Race.RiderIdentity>();

            // Add extra HUD text for position, lap, draft
            var hudCanvas = GameObject.Find("HUD Canvas");
            if (hudCanvas != null)
            {
                var hudPanel = hudCanvas.transform.Find("HUD Panel");
                if (hudPanel != null)
                {
                    // Expand panel height
                    var panelRT = hudPanel.GetComponent<RectTransform>();
                    panelRT.sizeDelta = new Vector2(320, 320);

                    var posText = CreateHUDText(hudPanel, "Position Text", "P--/--", new Vector2(15, -205));
                    var lapText = CreateHUDText(hudPanel, "Lap Text", "Lap 1/3", new Vector2(15, -235));
                    var draftText = CreateHUDText(hudPanel, "Draft Text", "", new Vector2(15, -265));

                    if (raceHUD != null)
                    {
                        var hudSO = new SerializedObject(raceHUD);
                        hudSO.FindProperty("positionText").objectReferenceValue = posText;
                        hudSO.FindProperty("lapText").objectReferenceValue = lapText;
                        hudSO.FindProperty("draftText").objectReferenceValue = draftText;
                        hudSO.ApplyModifiedProperties();
                    }
                }
            }

            Debug.Log("Race Manager, Drafting System, Position Tracker created and wired.");
        }

        static TextMeshProUGUI CreateHUDText(Transform parent, string name, string text, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(0, 32);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = Color.white;

            return tmp;
        }
    }
}
