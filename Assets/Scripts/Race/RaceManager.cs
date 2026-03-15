using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cycling.Core;
using Cycling.Cycling;
using Cycling.Data;
using Cycling.AI;
using Cycling.Track;

namespace Cycling.Race
{
    public enum RaceState
    {
        Setup,
        Countdown,
        Racing,
        Finished
    }

    public class RaceManager : MonoBehaviour
    {
        [Header("Track")]
        [SerializeField] TrackSpline trackSpline;

        [Header("Race Settings")]
        [SerializeField] int totalLaps = 3;
        [SerializeField] int aiRiderCount = 20;
        [SerializeField, Range(0f, 1f)] float difficulty = 0.5f;

        [Header("Data")]
        [SerializeField] RiderData[] aiRiderDataList;
        [SerializeField] AIStrategyData defaultStrategy;
        [SerializeField] BikeData defaultBike;

        [Header("Visuals")]
        [SerializeField] GameObject riderVisualPrefab;

        [Header("References")]
        [SerializeField] DraftingSystem draftingSystem;
        [SerializeField] PositionTracker positionTracker;
        [SerializeField] RiderMotor playerMotor;

        readonly List<GameObject> _spawnedRiders = new();
        readonly List<AIRiderBrain> _aiBrains = new();

        RaceState _state = RaceState.Setup;
        float _countdownTimer;
        bool _frozen = true;

        public PositionTracker PositionTracker => positionTracker;
        public int TotalLaps => totalLaps;
        public RaceState State => _state;
        public float CountdownTimer => _countdownTimer;

        public float Difficulty
        {
            get => difficulty;
            set
            {
                difficulty = Mathf.Clamp01(value);
                ApplyDifficulty();
            }
        }

        void Start()
        {
            // Read config from GameManager if available
            var gm = GameManager.Instance;
            if (gm != null)
            {
                totalLaps = gm.laps;
                aiRiderCount = gm.aiCount;
                difficulty = gm.difficulty;
                Debug.Log($"[RaceManager] Config from GameManager: laps={totalLaps}, AI={aiRiderCount}, difficulty={difficulty:P0}");
            }
            else
            {
                Debug.Log("[RaceManager] No GameManager found, using serialized defaults.");
            }

            PositionPlayerOnGrid();
            SpawnAIRiders();
            RegisterAllRiders();
            ApplyDifficulty();
            FreezeAll(true);

            _state = RaceState.Countdown;
            _countdownTimer = 4f; // 3-2-1-Go
            EventBus.RaceCountdown();
        }

        void Update()
        {
            switch (_state)
            {
                case RaceState.Countdown:
                    _countdownTimer -= Time.deltaTime;
                    if (_countdownTimer <= 0f)
                    {
                        _state = RaceState.Racing;
                        _frozen = false;
                        FreezeAll(false);
                        EventBus.RaceStart();
                    }
                    break;

                case RaceState.Racing:
                    CheckFinish();
                    break;
            }
        }

        void CheckFinish()
        {
            // Check if player finished
            if (playerMotor != null)
            {
                var lap = playerMotor.GetComponent<LapTracker>();
                if (lap != null && lap.Finished)
                {
                    _state = RaceState.Finished;
                    EventBus.RaceFinished();
                }
            }
        }

        int _playerGridSlot = -1;

        void PositionPlayerOnGrid()
        {
            if (playerMotor == null || trackSpline == null) return;

            int columns = 5;
            float rowSpacing = 3.5f;
            float lateralSpacing = 1.8f;
            float startOffset = 5f;
            int totalSlots = aiRiderCount + 1; // player + AI

            // Player gets a random slot in the grid
            _playerGridSlot = Random.Range(0, totalSlots);

            int col = _playerGridSlot % columns;
            int row = _playerGridSlot / columns;
            float lateralOffset = (col - (columns - 1) * 0.5f) * lateralSpacing;
            float dist = trackSpline.TotalLength - startOffset - (row * rowSpacing);
            dist = trackSpline.WrapDistance(dist);

            playerMotor.DistanceAlongSpline = dist;
            playerMotor.GridLateralOffset = lateralOffset;
        }

        void FreezeAll(bool freeze)
        {
            _frozen = freeze;

            // Freeze/unfreeze player — motor stays enabled for positioning
            if (playerMotor != null)
                playerMotor.Frozen = freeze;

            // Freeze/unfreeze AI
            foreach (var go in _spawnedRiders)
            {
                var motor = go.GetComponent<RiderMotor>();
                if (motor != null) motor.Frozen = freeze;
                var brain = go.GetComponent<AIRiderBrain>();
                if (brain != null) brain.enabled = !freeze;
            }
        }

        float DifficultyToFtpScale()
        {
            return Mathf.Lerp(0.36f, 1.36f, difficulty);
        }

        void ApplyDifficulty()
        {
            float scale = DifficultyToFtpScale();
            foreach (var brain in _aiBrains)
            {
                if (brain != null)
                    brain.SetDifficultyScale(scale);
            }
        }

        void SpawnAIRiders()
        {
            if (trackSpline == null) return;

            int columns = 5;
            float rowSpacing = 3.5f;     // distance between rows along track
            float lateralSpacing = 1.8f; // distance between riders in a column
            float startOffset = 5f;      // first row behind player

            var strategies = GenerateStrategies();

            int count = Mathf.Min(aiRiderCount, aiRiderDataList != null ? aiRiderDataList.Length : aiRiderCount);

            // Build grid slots, skipping the player's slot
            int totalSlots = count + 1; // player + AI
            var gridOrder = new System.Collections.Generic.List<int>();
            for (int i = 0; i < totalSlots; i++)
            {
                if (i != _playerGridSlot)
                    gridOrder.Add(i);
            }
            // Shuffle AI grid positions
            for (int i = gridOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (gridOrder[i], gridOrder[j]) = (gridOrder[j], gridOrder[i]);
            }

            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = $"AI Rider {i + 1}";

                int gridPos = gridOrder[i];
                int col = gridPos % columns;           // 0-4 across the road
                int row = gridPos / columns;           // 0-3 back from start

                // Position along spline: each row further back
                float dist = trackSpline.TotalLength - startOffset - (row * rowSpacing);
                dist = trackSpline.WrapDistance(dist);
                trackSpline.Evaluate(dist, out Vector3 pos, out Quaternion rot);

                // Lateral offset: 5 columns spread across the road
                float lateralOffset = (col - (columns - 1) * 0.5f) * lateralSpacing;
                Vector3 right = rot * Vector3.right;
                pos += right * lateralOffset;

                go.transform.SetPositionAndRotation(pos, rot);

                var motor = go.AddComponent<RiderMotor>();
                float bikeMass = defaultBike != null ? defaultBike.mass : 8f;
                float riderWeight = 72f;
                float riderCdA = defaultBike != null ? defaultBike.cdA : Constants.DefaultCdA;
                float riderCrr = defaultBike != null ? defaultBike.crr : Constants.DefaultCrr;

                var identity = go.AddComponent<RiderIdentity>();
                if (aiRiderDataList != null && i < aiRiderDataList.Length && aiRiderDataList[i] != null)
                {
                    identity.Init(aiRiderDataList[i], false);
                    riderWeight = aiRiderDataList[i].weight;
                }

                // Match player's vertical offset so AI rides at the same height
                float yOffset = playerMotor != null ? playerMotor.VerticalOffset : 1.3f;
                motor.Init(trackSpline, riderWeight + bikeMass, riderCdA, riderCrr, yOffset);

                // Set starting grid position
                motor.DistanceAlongSpline = dist;
                motor.GridLateralOffset = lateralOffset;

                var lap = go.AddComponent<LapTracker>();
                lap.Init(totalLaps);

                var brain = go.AddComponent<AIRiderBrain>();
                brain.Init(i < strategies.Length ? strategies[i] : defaultStrategy);
                _aiBrains.Add(brain);

                // Add visual model
                if (riderVisualPrefab != null)
                {
                    RiderVisual.SharedVisualPrefab = riderVisualPrefab;
                    go.AddComponent<RiderVisual>();
                }

                _spawnedRiders.Add(go);
            }
        }

        AIStrategyData[] GenerateStrategies()
        {
            int count = aiRiderCount;
            var strategies = new AIStrategyData[count];

            for (int i = 0; i < count; i++)
            {
                var s = ScriptableObject.CreateInstance<AIStrategyData>();
                s.aggressiveness = Random.Range(0.15f, 0.9f);
                s.sprintAbility = Random.Range(0.2f, 0.95f);
                s.climbingAbility = Random.Range(0.2f, 0.95f);
                s.powerVariation = Random.Range(0.03f, 0.08f);
                strategies[i] = s;
            }

            return strategies;
        }

        void RegisterAllRiders()
        {
            if (playerMotor != null)
            {
                if (draftingSystem != null) draftingSystem.Register(playerMotor);
                if (positionTracker != null) positionTracker.Register(playerMotor);
            }

            foreach (var go in _spawnedRiders)
            {
                var motor = go.GetComponent<RiderMotor>();
                if (motor != null)
                {
                    if (draftingSystem != null) draftingSystem.Register(motor);
                    if (positionTracker != null) positionTracker.Register(motor);
                }
            }
        }

        void FixedUpdate()
        {
            if (draftingSystem == null || _frozen) return;

            if (playerMotor != null)
                playerMotor.DraftFactor = draftingSystem.GetDraftFactor(playerMotor);

            foreach (var go in _spawnedRiders)
            {
                var motor = go.GetComponent<RiderMotor>();
                if (motor != null)
                    motor.DraftFactor = draftingSystem.GetDraftFactor(motor);
            }
        }
    }
}
