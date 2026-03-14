using UnityEngine;
using UnityEngine.InputSystem;

namespace Cycling.Camera
{
    public class RiderCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] Transform target;

        [Header("Follow Settings")]
        [SerializeField] float positionSmoothing = 5f;
        [SerializeField] float rotationSmoothing = 8f;

        [Header("Camera Presets")]
        [SerializeField] Vector3 closeOffset = new Vector3(0f, 1.8f, -3.5f);
        [SerializeField] Vector3 wideOffset = new Vector3(0f, 3.5f, -8f);
        [SerializeField] Vector3 overheadOffset = new Vector3(0f, 12f, -4f);

        int _presetIndex;
        Vector3 _currentOffset;

        static readonly string[] PresetNames = { "Close", "Wide", "Overhead" };

        void Start()
        {
            _currentOffset = closeOffset;
        }

        void Update()
        {
            // C key toggles camera preset
            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            {
                _presetIndex = (_presetIndex + 1) % 3;
                Debug.Log($"Camera: {PresetNames[_presetIndex]}");
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetOffset = _presetIndex switch
            {
                0 => closeOffset,
                1 => wideOffset,
                2 => overheadOffset,
                _ => closeOffset
            };

            _currentOffset = Vector3.Lerp(_currentOffset, targetOffset, 3f * Time.deltaTime);

            Vector3 desiredPosition = target.position
                + target.forward * _currentOffset.z
                + target.up * _currentOffset.y
                + target.right * _currentOffset.x;

            transform.position = Vector3.Lerp(transform.position, desiredPosition,
                positionSmoothing * Time.deltaTime);

            // Look at a point slightly above the rider
            float lookHeight = _presetIndex == 2 ? 0f : 1f;
            Quaternion desiredRotation = Quaternion.LookRotation(
                target.position + target.up * lookHeight - transform.position, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation,
                rotationSmoothing * Time.deltaTime);
        }
    }
}
