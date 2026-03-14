using UnityEngine;
using Cycling.Cycling;
using Cycling.Race;

namespace Cycling.AI
{
    public enum AIState
    {
        Peloton,
        Chase,
        Breakaway,
        Sprint,
        Gruppetto
    }

    [RequireComponent(typeof(RiderMotor), typeof(RiderIdentity))]
    public class AIRiderBrain : MonoBehaviour
    {
        [SerializeField] AIStrategyData strategy;

        RiderMotor _motor;
        RiderIdentity _identity;
        LapTracker _lapTracker;

        // Power
        float _ftp;
        float _baseCruisePower;
        float _noiseOffset;
        float _noiseTimer;

        // State machine
        AIState _state = AIState.Peloton;
        float _stateTimer;
        float _attackCooldown;

        // Fatigue
        float _matchesRemaining = 1f; // 1.0 = fresh, 0 = spent
        const float MatchBurnRate = 0.008f;   // per second above threshold
        const float MatchRecoveryRate = 0.003f; // per second below threshold

        // Overtaking
        float _overtakeTimer;
        RiderMotor _riderAhead;

        // Difficulty
        float _difficultyFtpScale = 1f;

        public AIState State => _state;

        public void Init(AIStrategyData strategyData)
        {
            strategy = strategyData;
        }

        public void SetDifficultyScale(float scale)
        {
            _difficultyFtpScale = scale;
            RecalculateBasePower();
        }

        void Awake()
        {
            _motor = GetComponent<RiderMotor>();
            _identity = GetComponent<RiderIdentity>();
            _lapTracker = GetComponent<LapTracker>();
            _noiseOffset = Random.Range(0f, 100f);
        }

        void Start()
        {
            _ftp = _identity.FTP;
            RecalculateBasePower();
            _attackCooldown = Random.Range(15f, 40f);
        }

        void RecalculateBasePower()
        {
            float scaledFtp = _ftp * _difficultyFtpScale;
            float cruiseFraction = 0.72f + (strategy != null ? strategy.aggressiveness * 0.13f : 0f);
            _baseCruisePower = scaledFtp * cruiseFraction;
        }

        void Update()
        {
            if (_motor == null) return;

            float dt = Time.deltaTime;
            float scaledFtp = _ftp * _difficultyFtpScale;

            UpdateState(dt, scaledFtp);
            float power = CalculatePower(dt, scaledFtp);
            UpdateFatigue(dt, power, scaledFtp);
            UpdateOvertaking(dt);

            _motor.PowerWatts = Mathf.Max(power, 50f);
        }

        void UpdateState(float dt, float scaledFtp)
        {
            _stateTimer += dt;
            _attackCooldown -= dt;

            float aggressiveness = strategy != null ? strategy.aggressiveness : 0.5f;
            bool isLastLap = _lapTracker != null && _lapTracker.CurrentLap >= _lapTracker.TotalLaps - 1;

            switch (_state)
            {
                case AIState.Peloton:
                    // Consider attacking
                    if (_attackCooldown <= 0 && _matchesRemaining > 0.5f && aggressiveness > 0.3f)
                    {
                        float attackChance = aggressiveness * 0.02f * dt;
                        if (Random.value < attackChance)
                        {
                            _state = AIState.Breakaway;
                            _stateTimer = 0f;
                        }
                    }
                    // Sprint on last lap when close to finish
                    if (isLastLap && _motor.DistanceAlongSpline > _motor.TrackLength * 0.85f && _matchesRemaining > 0.2f)
                    {
                        _state = AIState.Sprint;
                        _stateTimer = 0f;
                    }
                    break;

                case AIState.Breakaway:
                    // Sustain breakaway for 10-30 seconds depending on matches
                    float maxBreakDuration = 10f + aggressiveness * 20f;
                    if (_stateTimer > maxBreakDuration || _matchesRemaining < 0.15f)
                    {
                        _state = AIState.Peloton;
                        _stateTimer = 0f;
                        _attackCooldown = Random.Range(30f, 60f);
                    }
                    // Switch to sprint if last lap and near finish
                    if (isLastLap && _motor.DistanceAlongSpline > _motor.TrackLength * 0.85f)
                    {
                        _state = AIState.Sprint;
                        _stateTimer = 0f;
                    }
                    break;

                case AIState.Sprint:
                    // Sprint until out of matches or crossed finish
                    if (_matchesRemaining < 0.05f)
                    {
                        _state = AIState.Gruppetto;
                        _stateTimer = 0f;
                    }
                    break;

                case AIState.Gruppetto:
                    // Recover slowly, rejoin peloton if matches recover
                    if (_matchesRemaining > 0.4f)
                    {
                        _state = AIState.Peloton;
                        _stateTimer = 0f;
                    }
                    break;
            }
        }

        float CalculatePower(float dt, float scaledFtp)
        {
            // Perlin noise for natural variation
            _noiseTimer += dt * 0.3f;
            float variation = strategy != null ? strategy.powerVariation : 0.05f;
            float noise = Mathf.PerlinNoise(_noiseTimer, _noiseOffset) * 2f - 1f;

            float power;

            switch (_state)
            {
                case AIState.Peloton:
                    power = _baseCruisePower * (1f + noise * variation);
                    break;

                case AIState.Breakaway:
                    // Push to ~105-115% FTP
                    float breakPower = scaledFtp * (1.05f + (strategy != null ? strategy.aggressiveness * 0.1f : 0f));
                    power = breakPower * (1f + noise * variation * 0.5f);
                    break;

                case AIState.Sprint:
                    // All out: 130-160% FTP
                    float sprintAbility = strategy != null ? strategy.sprintAbility : 0.5f;
                    float sprintPower = scaledFtp * (1.3f + sprintAbility * 0.3f);
                    power = sprintPower * (1f + noise * 0.03f);
                    break;

                case AIState.Gruppetto:
                    // Easy riding: 55-65% FTP
                    power = scaledFtp * 0.6f * (1f + noise * variation);
                    break;

                default:
                    power = _baseCruisePower;
                    break;
            }

            // Climbing boost
            float gradient = _motor.Gradient;
            if (gradient > 0.02f && strategy != null)
            {
                float climbBoost = strategy.climbingAbility * 0.15f * gradient * 10f;
                power *= (1f + climbBoost);
            }

            // Fatigue: reduce max power when matches are low
            if (_matchesRemaining < 0.3f)
            {
                float fatiguePenalty = Mathf.InverseLerp(0f, 0.3f, _matchesRemaining);
                float maxPower = Mathf.Lerp(scaledFtp * 0.6f, scaledFtp * 1.5f, fatiguePenalty);
                power = Mathf.Min(power, maxPower);
            }

            return power;
        }

        void UpdateFatigue(float dt, float power, float scaledFtp)
        {
            if (power > scaledFtp)
            {
                float intensity = (power - scaledFtp) / scaledFtp;
                _matchesRemaining -= MatchBurnRate * intensity * dt;
            }
            else
            {
                float recovery = (scaledFtp - power) / scaledFtp;
                _matchesRemaining += MatchRecoveryRate * recovery * dt;
            }

            _matchesRemaining = Mathf.Clamp01(_matchesRemaining);
        }

        void UpdateOvertaking(float dt)
        {
            // Check if we're close behind someone and faster
            var drafting = Object.FindFirstObjectByType<DraftingSystem>();
            if (drafting == null) return;

            float myDist = _motor.DistanceAlongSpline;
            float mySpeed = _motor.SpeedMs;
            float trackLen = _motor.TrackLength;

            // Find nearest rider ahead within 4m
            RiderMotor nearest = null;
            float nearestGap = float.MaxValue;

            var allMotors = Object.FindObjectsByType<RiderMotor>(FindObjectsSortMode.None);
            foreach (var other in allMotors)
            {
                if (other == _motor) continue;
                float gap = other.DistanceAlongSpline - myDist;
                if (gap < 0) gap += trackLen;
                if (gap > 0 && gap < 4f && gap < nearestGap)
                {
                    nearestGap = gap;
                    nearest = other;
                }
            }

            if (nearest != null && mySpeed > nearest.SpeedMs + 0.3f && nearestGap < 3f)
            {
                // Pull out to overtake
                _motor.TargetLateralOffset = 1.5f;
                _overtakeTimer = 2f;
            }
            else if (_overtakeTimer > 0)
            {
                _overtakeTimer -= dt;
            }
            else
            {
                // Return to line
                _motor.TargetLateralOffset = 0f;
            }
        }
    }
}
