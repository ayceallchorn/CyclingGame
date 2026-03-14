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

        void Update()
        {
            if (_riders.Count == 0 || trackSpline == null) return;

            _sorted.Clear();
            _sorted.AddRange(_riders);

            float trackLen = trackSpline.TotalLength;

            // Sort by effective distance (laps * trackLength + distance)
            _sorted.Sort((a, b) =>
            {
                var lapA = a.GetComponent<LapTracker>();
                var lapB = b.GetComponent<LapTracker>();
                float effA = (lapA != null ? lapA.CurrentLap : 0) * trackLen + a.DistanceAlongSpline;
                float effB = (lapB != null ? lapB.CurrentLap : 0) * trackLen + b.DistanceAlongSpline;
                return effB.CompareTo(effA); // descending — leader first
            });

            if (_positionIds == null || _positionIds.Length != _sorted.Count)
                _positionIds = new int[_sorted.Count];

            for (int i = 0; i < _sorted.Count; i++)
                _positionIds[i] = _sorted[i].gameObject.GetInstanceID();

            EventBus.PositionsUpdated(_positionIds);
        }
    }
}
