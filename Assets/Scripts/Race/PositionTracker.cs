using System.Collections.Generic;
using UnityEngine;
using Cycling.Core;
using Cycling.Cycling;
using Cycling.Track;

namespace Cycling.Race
{
    public class PositionTracker : MonoBehaviour
    {
        [SerializeField] TrackSpline trackSpline;

        readonly List<RiderMotor> _riders = new();
        readonly List<RiderMotor> _sorted = new();
        int[] _positionIds;

        public void Register(RiderMotor rider) => _riders.Add(rider);
        public void Unregister(RiderMotor rider) => _riders.Remove(rider);

        /// <summary>
        /// Returns 1-based position for a rider. Returns -1 if not found.
        /// </summary>
        public int GetPosition(RiderMotor rider)
        {
            int idx = _sorted.IndexOf(rider);
            return idx >= 0 ? idx + 1 : -1;
        }

        public int TotalRiders => _riders.Count;
        public IReadOnlyList<RiderMotor> SortedRiders => _sorted;

        public float GetEffectiveDistance(RiderMotor rider)
        {
            if (trackSpline == null) return 0f;
            var lap = rider.GetComponent<LapTracker>();
            return (lap != null ? lap.CurrentLap : 0) * trackSpline.TotalLength + rider.DistanceAlongSpline;
        }

        void Update()
        {
            if (_riders.Count == 0 || trackSpline == null) return;

            _sorted.Clear();
            _sorted.AddRange(_riders);

            // Sort by effective distance — leader first
            _sorted.Sort((a, b) => GetEffectiveDistance(b).CompareTo(GetEffectiveDistance(a)));

            if (_positionIds == null || _positionIds.Length != _sorted.Count)
                _positionIds = new int[_sorted.Count];

            for (int i = 0; i < _sorted.Count; i++)
                _positionIds[i] = _sorted[i].gameObject.GetInstanceID();

            EventBus.PositionsUpdated(_positionIds);
        }
    }
}
