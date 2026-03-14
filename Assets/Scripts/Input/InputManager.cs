using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cycling.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] InputActionAsset actionAsset;

        InputActionMap _gameplay;
        InputAction _shiftUp;
        InputAction _shiftDown;
        InputAction _toggleDebug;
        InputAction _pause;

        public event Action OnShiftUp;
        public event Action OnShiftDown;
        public event Action OnToggleDebug;
        public event Action OnPause;

        public static InputManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _gameplay = actionAsset.FindActionMap("Gameplay");
            _shiftUp = _gameplay.FindAction("ShiftUp");
            _shiftDown = _gameplay.FindAction("ShiftDown");
            _toggleDebug = _gameplay.FindAction("ToggleDebug");
            _pause = _gameplay.FindAction("Pause");
        }

        void OnEnable()
        {
            _gameplay.Enable();
            _shiftUp.performed += ctx => OnShiftUp?.Invoke();
            _shiftDown.performed += ctx => OnShiftDown?.Invoke();
            _toggleDebug.performed += ctx => OnToggleDebug?.Invoke();
            _pause.performed += ctx => OnPause?.Invoke();
        }

        void OnDisable()
        {
            _gameplay.Disable();
        }
    }
}
