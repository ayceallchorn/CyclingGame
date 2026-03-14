using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace Cycling.Track
{
    [RequireComponent(typeof(SplineContainer))]
    public class TrackSpline : MonoBehaviour
    {
        SplineContainer _container;
        float _totalLength;

        public float TotalLength => _totalLength;

        void Awake()
        {
            _container = GetComponent<SplineContainer>();
            _totalLength = _container.CalculateLength();
        }

        /// <summary>
        /// Evaluates position and rotation at a given distance along the spline.
        /// Distance wraps around for closed splines.
        /// </summary>
        public void Evaluate(float distance, out Vector3 position, out Quaternion rotation)
        {
            float t = DistanceToNormalized(distance);

            _container.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);

            position = transform.TransformPoint((Vector3)pos);

            if (math.lengthsq(tangent) > 0.0001f)
            {
                Vector3 fwd = transform.TransformDirection(math.normalize(tangent));
                Vector3 upDir = transform.TransformDirection(math.normalize(up));
                rotation = Quaternion.LookRotation(fwd, upDir);
            }
            else
            {
                rotation = transform.rotation;
            }
        }

        /// <summary>
        /// Returns the gradient (rise/run) at a given distance along the spline.
        /// Positive = uphill, negative = downhill.
        /// </summary>
        public float GetGradient(float distance)
        {
            float t = DistanceToNormalized(distance);

            _container.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);

            Vector3 worldTangent = transform.TransformDirection((Vector3)tangent);
            if (worldTangent.sqrMagnitude < 0.0001f) return 0f;

            worldTangent.Normalize();
            // Gradient = vertical component / horizontal component
            float horizontal = Mathf.Sqrt(worldTangent.x * worldTangent.x + worldTangent.z * worldTangent.z);
            if (horizontal < 0.0001f) return worldTangent.y > 0 ? 1f : -1f;

            return worldTangent.y / horizontal;
        }

        /// <summary>
        /// Wraps a distance value to [0, TotalLength).
        /// </summary>
        public float WrapDistance(float distance)
        {
            if (_totalLength <= 0f) return 0f;
            distance %= _totalLength;
            if (distance < 0f) distance += _totalLength;
            return distance;
        }

        float DistanceToNormalized(float distance)
        {
            if (_totalLength <= 0f) return 0f;
            float wrapped = WrapDistance(distance);
            return wrapped / _totalLength;
        }
    }
}
