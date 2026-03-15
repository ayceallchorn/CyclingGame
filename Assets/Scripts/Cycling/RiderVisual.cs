using UnityEngine;

namespace Cycling.Cycling
{
    /// <summary>
    /// Replaces the capsule mesh with a bike+rider visual prefab.
    /// Attach to any rider GameObject (player or AI).
    /// </summary>
    public class RiderVisual : MonoBehaviour
    {
        [SerializeField] GameObject visualPrefab;
        [SerializeField] Vector3 visualOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] Vector3 visualRotationOffset = Vector3.zero;
        [SerializeField] float visualScale = 1f;

        GameObject _visualInstance;
        Animator _pedalAnimator;
        bool _isPlayer;

        public static GameObject SharedVisualPrefab { get; set; }

        void Awake()
        {
            // Do setup in Awake so it happens before any physics tick
            var prefab = visualPrefab != null ? visualPrefab : SharedVisualPrefab;
            if (prefab == null) return;

            // Hide the capsule mesh
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null) meshFilter.mesh = null;

            // Instantiate visual as child
            _visualInstance = Instantiate(prefab, transform);
            _visualInstance.name = "BikeVisual";
            _visualInstance.transform.localPosition = visualOffset;
            _visualInstance.transform.localRotation = Quaternion.Euler(visualRotationOffset);
            _visualInstance.transform.localScale = Vector3.one * visualScale;

            // Strip unwanted components IMMEDIATELY before physics can act
            StripUnwantedComponents(_visualInstance);
        }

        void Start()
        {
            // Cache the pedal animator (on "Pedalier" child, not the rig root)
            if (_visualInstance != null)
            {
                foreach (var anim in _visualInstance.GetComponentsInChildren<Animator>(true))
                {
                    if (anim.runtimeAnimatorController != null)
                    {
                        _pedalAnimator = anim;
                        break;
                    }
                }
            }

            // Check if this is the player
            var identity = GetComponent<Race.RiderIdentity>();
            _isPlayer = identity != null && identity.IsPlayer;

            // Apply team colour after RiderIdentity.Init has run
            if (identity != null)
                ApplyTeamColour(identity.TeamColor);
        }

        void Update()
        {
            if (_pedalAnimator == null) return;

            float cadence;

            if (_isPlayer)
            {
                // Player cadence from debug panel
                var debug = UI.DebugPanel.Instance;
                cadence = debug != null ? debug.SimulatedCadence : 90f;
            }
            else
            {
                // AI cadence: derive from power and speed
                // Rough approximation: cadence ≈ 70-100 rpm, scaled by power output
                var motor = GetComponent<RiderMotor>();
                if (motor != null && motor.SpeedMs > 0.5f)
                    cadence = Mathf.Lerp(60f, 110f, Mathf.InverseLerp(100f, 350f, motor.PowerWatts));
                else
                    cadence = 0f;
            }

            // Animation speed: 1.0 = one full pedal rotation per second = 60 RPM
            _pedalAnimator.speed = cadence / 60f;
        }

        void StripUnwantedComponents(GameObject go)
        {
            // MUST destroy scripts first — Rigidbody can't be removed while BicycleVehicle depends on it
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string typeName = mb.GetType().FullName ?? "";
                if ((typeName.StartsWith("rayzngames") || typeName.StartsWith("RayznGames"))
                    && !typeName.Contains("BikeIKTargets"))
                    DestroyImmediate(mb);
            }

            // Remove physics: colliders first (WheelCollider depends on Rigidbody),
            // then joints, then rigidbodies
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                DestroyImmediate(col);
            foreach (var joint in go.GetComponentsInChildren<Joint>(true))
                DestroyImmediate(joint);
            foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
                DestroyImmediate(rb);
        }

        void ApplyTeamColour(Color color)
        {
            if (_visualInstance == null) return;

            foreach (var rend in _visualInstance.GetComponentsInChildren<Renderer>(true))
            {
                var mat = rend.material;
                mat.color = Color.Lerp(mat.color, color, 0.6f);
            }
        }
    }
}
