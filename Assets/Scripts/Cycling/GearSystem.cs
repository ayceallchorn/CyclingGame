using UnityEngine;
using Cycling.Data;
using Cycling.Input;

namespace Cycling.Cycling
{
    public class GearSystem : MonoBehaviour
    {
        [SerializeField] GearTableData gearTable;

        int _currentIndex;
        float[] _ratios;

        public int CurrentGearIndex => _currentIndex + 1; // 1-based for display
        public int TotalGears => _ratios?.Length ?? 0;
        public float CurrentRatio => _ratios != null ? _ratios[_currentIndex] : 1f;
        public GearTableData GearTable => gearTable;

        /// <summary>
        /// Speed in m/s for a given cadence (RPM) at the current gear.
        /// </summary>
        public float SpeedFromCadence(float cadenceRpm)
        {
            if (gearTable == null) return 0f;
            // wheel RPM = cadence * gear ratio
            // speed = wheel RPM * circumference / 60
            return cadenceRpm * CurrentRatio * gearTable.wheelCircumference / 60f;
        }

        void Awake()
        {
            if (gearTable != null)
            {
                _ratios = gearTable.GetSortedRatios();
                // Start in a middle gear
                _currentIndex = _ratios.Length / 2;
            }
        }

        void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnShiftUp += ShiftUp;
                InputManager.Instance.OnShiftDown += ShiftDown;
            }
        }

        void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnShiftUp -= ShiftUp;
                InputManager.Instance.OnShiftDown -= ShiftDown;
            }
        }

        public void ShiftUp()
        {
            if (_ratios == null) return;
            _currentIndex = Mathf.Min(_currentIndex + 1, _ratios.Length - 1);
        }

        public void ShiftDown()
        {
            if (_ratios == null) return;
            _currentIndex = Mathf.Max(_currentIndex - 1, 0);
        }
    }
}
