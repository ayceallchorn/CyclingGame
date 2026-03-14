using UnityEngine;
using Cycling.Core;
using Cycling.Cycling;
using Cycling.Track;

namespace Cycling.Race
{
    [RequireComponent(typeof(RiderMotor))]
    public class LapTracker : MonoBehaviour
    {
        [SerializeField] int totalLaps = 3;

        RiderMotor _motor;
        float _prevDistance;
        int _currentLap;
        bool _finished;

        public int CurrentLap => _currentLap;
        public int TotalLaps => totalLaps;
        public bool Finished => _finished;

        public void Init(int laps)
        {
            totalLaps = laps;
        }

        void Awake()
        {
            _motor = GetComponent<RiderMotor>();
        }

        void Update()
        {
            if (_finished) return;

            float dist = _motor.DistanceAlongSpline;

            // Detect wrap-around: previous distance was near end, current is near start
            if (_prevDistance > 0 && _prevDistance - dist > _motor.TrackLength * 0.5f)
            {
                _currentLap++;
                EventBus.LapCompleted(gameObject.GetInstanceID());

                if (_currentLap >= totalLaps)
                    _finished = true;
            }

            _prevDistance = dist;
        }
    }
}
