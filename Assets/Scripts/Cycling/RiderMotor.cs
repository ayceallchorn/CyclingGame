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
        [SerializeField] float verticalOffset = 4.1f;
        public float VerticalOffset { get => verticalOffset; set => verticalOffset = value; }
        [SerializeField] float lateralOffsetMax = 1.5f;
        [SerializeField] float lateralLerpSpeed = 3f;

        [Header("State (Read Only)")]
        [SerializeField] float currentSpeedMs;
        [SerializeField] float distanceAlongSpline;
        [SerializeField] float currentGradient;
        [SerializeField] float currentDraftFactor;
        [SerializeField] float currentLateralOffset;

        float _targetLateralOffset;

        public float SpeedMs => currentSpeedMs;
        public float SpeedKmh => currentSpeedMs * 3.6f;
        public float DistanceAlongSpline => distanceAlongSpline;
        public float Gradient => currentGradient;
        public float PowerWatts { get => powerWatts; set => powerWatts = value; }
        public float TrackLength => trackSpline != null ? trackSpline.TotalLength : 0f;
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
        }

        void FixedUpdate()
        {
            if (trackSpline == null) return;

            currentGradient = trackSpline.GetGradient(distanceAlongSpline);

            float effectiveCdA = cdA * (1f - currentDraftFactor);

            float acceleration = CyclingPhysics.CalculateAcceleration(
                powerWatts, currentSpeedMs, currentGradient, massKg, effectiveCdA, crr);

            currentSpeedMs += acceleration * Time.fixedDeltaTime;
            currentSpeedMs = Mathf.Clamp(currentSpeedMs, Constants.MinSpeed, Constants.MaxSpeed);

            distanceAlongSpline += currentSpeedMs * Time.fixedDeltaTime;
            distanceAlongSpline = trackSpline.WrapDistance(distanceAlongSpline);

            // Smooth lateral offset
            currentLateralOffset = Mathf.Lerp(currentLateralOffset, _targetLateralOffset,
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
