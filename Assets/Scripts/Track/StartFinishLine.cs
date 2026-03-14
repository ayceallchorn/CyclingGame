using UnityEngine;
using Cycling.Core;

namespace Cycling.Track
{
    /// <summary>
    /// Start/finish line with ground markings, translucent arch, and crossing effect.
    /// Visuals are built by the editor TrackSceneBuilder — this script handles runtime effects only.
    /// </summary>
    public class StartFinishLine : MonoBehaviour
    {
        [SerializeField] TrackSpline trackSpline;
        [SerializeField] float roadWidth = 10f;
        [SerializeField] float archHeight = 5f;

        ParticleSystem _burstParticles;

        void Start()
        {
            // Find the particle system (created by builder)
            _burstParticles = GetComponentInChildren<ParticleSystem>();
            EventBus.OnLapCompleted += OnLapCompleted;
        }

        void OnDestroy()
        {
            EventBus.OnLapCompleted -= OnLapCompleted;
        }

        void OnLapCompleted(int riderInstanceId)
        {
            if (_burstParticles != null)
                _burstParticles.Play();
        }
    }
}
