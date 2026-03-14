using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cycling.Input
{
    public class InputManager : MonoBehaviour
    {
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
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.eKey.wasPressedThisFrame) OnShiftUp?.Invoke();
            if (kb.qKey.wasPressedThisFrame) OnShiftDown?.Invoke();
            if (kb.backquoteKey.wasPressedThisFrame) OnToggleDebug?.Invoke(); // ` key
            if (kb.escapeKey.wasPressedThisFrame) OnPause?.Invoke();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
