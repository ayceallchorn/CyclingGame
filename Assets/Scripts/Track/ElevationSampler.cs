using UnityEngine;

namespace Cycling.Track
{
    /// <summary>
    /// Pre-samples elevation and gradient along a TrackSpline at regular intervals.
    /// Useful for UI (elevation profile display, minimap) and AI lookahead.
    /// </summary>
    public class ElevationSampler : MonoBehaviour
    {
        [SerializeField] TrackSpline trackSpline;
        [SerializeField] int sampleCount = 200;

        float[] _elevations;
        float[] _gradients;
        float _sampleSpacing;

        public int SampleCount => sampleCount;
        public float SampleSpacing => _sampleSpacing;

        /// <summary>
        /// Returns the pre-sampled elevation at a given distance along the spline.
        /// </summary>
        public float GetElevation(float distance)
        {
            if (_elevations == null || _elevations.Length == 0) return 0f;
            float wrapped = trackSpline.WrapDistance(distance);
            float indexF = wrapped / _sampleSpacing;
            int i0 = Mathf.FloorToInt(indexF) % sampleCount;
            int i1 = (i0 + 1) % sampleCount;
            float t = indexF - Mathf.Floor(indexF);
            return Mathf.Lerp(_elevations[i0], _elevations[i1], t);
        }

        /// <summary>
        /// Returns the pre-sampled gradient at a given distance along the spline.
        /// </summary>
        public float GetGradient(float distance)
        {
            if (_gradients == null || _gradients.Length == 0) return 0f;
            float wrapped = trackSpline.WrapDistance(distance);
            float indexF = wrapped / _sampleSpacing;
            int i0 = Mathf.FloorToInt(indexF) % sampleCount;
            int i1 = (i0 + 1) % sampleCount;
            float t = indexF - Mathf.Floor(indexF);
            return Mathf.Lerp(_gradients[i0], _gradients[i1], t);
        }

        /// <summary>
        /// Returns min and max elevation for the whole track.
        /// </summary>
        public (float min, float max) GetElevationRange()
        {
            if (_elevations == null || _elevations.Length == 0) return (0f, 0f);
            float min = float.MaxValue, max = float.MinValue;
            foreach (float e in _elevations)
            {
                if (e < min) min = e;
                if (e > max) max = e;
            }
            return (min, max);
        }

        void Start()
        {
            RebuildSamples();
        }

        public void RebuildSamples()
        {
            if (trackSpline == null) return;

            _elevations = new float[sampleCount];
            _gradients = new float[sampleCount];
            _sampleSpacing = trackSpline.TotalLength / sampleCount;

            for (int i = 0; i < sampleCount; i++)
            {
                float dist = i * _sampleSpacing;
                trackSpline.Evaluate(dist, out Vector3 pos, out _);
                _elevations[i] = pos.y;
                _gradients[i] = trackSpline.GetGradient(dist);
            }
        }
    }
}
