using System.Collections.Generic;
using UnityEngine;
using Cycling.Track;

namespace Cycling.Cycling
{
    public class DraftingSystem : MonoBehaviour
    {
        [SerializeField] TrackSpline trackSpline;

        [Header("Drafting Settings")]
        [SerializeField] float maxDraftDistance = 5f;   // metres behind to get any draft
        [SerializeField] float optimalDraftDistance = 1.5f; // metres for max single-rider draft
        [SerializeField] float singleRiderDraft = 0.30f;   // 30% CdA reduction behind one rider
        [SerializeField] float pelotonDraftMax = 0.45f;     // 45% max in a group

        readonly List<RiderMotor> _riders = new();
        readonly Dictionary<RiderMotor, float> _draftFactors = new();

        public float GetDraftFactor(RiderMotor rider)
        {
            return _draftFactors.TryGetValue(rider, out float f) ? f : 0f;
        }

        public void Register(RiderMotor rider) => _riders.Add(rider);
        public void Unregister(RiderMotor rider) { _riders.Remove(rider); _draftFactors.Remove(rider); }

        void FixedUpdate()
        {
            if (trackSpline == null || _riders.Count < 2) return;

            float trackLen = trackSpline.TotalLength;

            // Sort riders by distance (front to back)
            _riders.Sort((a, b) => b.DistanceAlongSpline.CompareTo(a.DistanceAlongSpline));

            for (int i = 0; i < _riders.Count; i++)
            {
                float myDist = _riders[i].DistanceAlongSpline;
                int ridersAhead = 0;
                float bestDraftSingle = 0f;

                // Check riders ahead (earlier in sorted list = further ahead)
                for (int j = i - 1; j >= 0; j--)
                {
                    float aheadDist = _riders[j].DistanceAlongSpline;
                    float gap = aheadDist - myDist;

                    // Handle wrap-around
                    if (gap < 0) gap += trackLen;
                    if (gap > maxDraftDistance) break;

                    ridersAhead++;

                    // Single rider draft based on distance
                    float t = Mathf.InverseLerp(maxDraftDistance, optimalDraftDistance, gap);
                    float draft = Mathf.Lerp(0f, singleRiderDraft, t);
                    if (draft > bestDraftSingle) bestDraftSingle = draft;
                }

                // Group bonus: more riders ahead = more draft, up to peloton max
                float groupBonus = 0f;
                if (ridersAhead > 1)
                {
                    float extra = Mathf.Min(ridersAhead - 1, 5) / 5f;
                    groupBonus = extra * (pelotonDraftMax - singleRiderDraft);
                }

                _draftFactors[_riders[i]] = Mathf.Min(bestDraftSingle + groupBonus, pelotonDraftMax);
            }
        }
    }
}
