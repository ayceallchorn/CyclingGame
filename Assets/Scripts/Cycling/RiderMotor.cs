using UnityEngine;
using Cycling.Core;
using Cycling.Track;

namespace Cycling.Cycling
{
    public class RiderMotor : MonoBehaviour
    {
        [Header("Track")]
        [SerializeField] TrackSpline trackSpline;

        [Header("Rider Stats")]
        [SerializeField] float massKg = Constants.DefaultRiderMass;
        [SerializeField] float cdA = Constants.DefaultCdA;
        [SerializeField] float crr = Constants.DefaultCrr;

        [Header("Debug Input")]
        [SerializeField] float powerWatts = 200f;

        [Header("Positioning")]
        [SerializeField] float verticalOffset = 1.3f;
        public float VerticalOffset { get => verticalOffset; set => verticalOffset = value; }
        [SerializeField] float lateralOffsetMax = 4f;
        [SerializeField] float lateralLerpSpeed = 3f;

        [Header("State (Read Only)")]
        [SerializeField] float currentSpeedMs;
        [SerializeField] float distanceAlongSpline;
        [SerializeField] float currentGradient;
        [SerializeField] float currentDraftFactor;
        [SerializeField] float currentLateralOffset;

        float _targetLateralOffset;
        float _gridLateralOffset;
        float _gridMergeTimer;

        /// <summary>
        /// Starting grid lateral offset. Gradually merges to 0 after race starts.
        /// </summary>
        public float GridLateralOffset { get => _gridLateralOffset; set => _gridLateralOffset = value; }

        public float SpeedMs => currentSpeedMs;
        public float SpeedKmh => currentSpeedMs * 3.6f;
        public float DistanceAlongSpline { get => distanceAlongSpline; set => distanceAlongSpline = value; }
        public float Gradient => currentGradient;
        public float PowerWatts { get => powerWatts; set => powerWatts = value; }
        public float TrackLength => trackSpline != null ? trackSpline.TotalLength : 0f;
        public bool Frozen { get; set; }
        public TrackSpline TrackSpline => trackSpline;
        public float DraftFactor { get => currentDraftFactor; set => currentDraftFactor = value; }

        /// <summary>
        /// Set desired lateral offset for overtaking. Positive = left, negative = right.
        /// </summary>
        public float TargetLateralOffset { get => _targetLateralOffset; set => _targetLateralOffset = Mathf.Clamp(value, -lateralOffsetMax, lateralOffsetMax); }

        public void Init(TrackSpline spline, float mass, float dragCdA, float rollingCrr, float yOffset = -1f)
        {
            trackSpline = spline;
            massKg = mass;
            cdA = dragCdA;
            crr = rollingCrr;
            if (yOffset >= 0f) VerticalOffset = yOffset;
        }

        void Start()
        {
            currentSpeedMs = 2f;
            // Initialise lateral offset to grid position so no snap on first frame
            currentLateralOffset = _gridLateralOffset;
        }

        void FixedUpdate()
        {
            if (trackSpline == null) return;

            currentGradient = trackSpline.GetGradient(distanceAlongSpline);

            if (!Frozen)
            {
                float effectiveCdA = cdA * (1f - currentDraftFactor);

                float acceleration = CyclingPhysics.CalculateAcceleration(
                    powerWatts, currentSpeedMs, currentGradient, massKg, effectiveCdA, crr);

                currentSpeedMs += acceleration * Time.fixedDeltaTime;
                currentSpeedMs = Mathf.Clamp(currentSpeedMs, Constants.MinSpeed, Constants.MaxSpeed);

                distanceAlongSpline += currentSpeedMs * Time.fixedDeltaTime;
                distanceAlongSpline = trackSpline.WrapDistance(distanceAlongSpline);

                // Merge grid offset to 0 gradually after race starts
                if (Mathf.Abs(_gridLateralOffset) > 0.01f)
                {
                    _gridMergeTimer += Time.fixedDeltaTime;
                    if (_gridMergeTimer > 3f)
                        _gridLateralOffset = Mathf.MoveTowards(_gridLateralOffset, 0f, 0.4f * Time.fixedDeltaTime);
                }
            }

            // Always position on spline (even when frozen, for correct height)
            float totalTarget = _targetLateralOffset + _gridLateralOffset;
            currentLateralOffset = Mathf.Lerp(currentLateralOffset, totalTarget,
                lateralLerpSpeed * Time.fixedDeltaTime);

            trackSpline.Evaluate(distanceAlongSpline, out Vector3 pos, out Quaternion rot);

            // Lift rider above road surface
            pos += rot * Vector3.up * VerticalOffset;

            // Apply lateral offset perpendicular to track direction
            if (Mathf.Abs(currentLateralOffset) > 0.01f)
            {
                Vector3 right = rot * Vector3.right;
                pos += right * currentLateralOffset;
            }

            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
