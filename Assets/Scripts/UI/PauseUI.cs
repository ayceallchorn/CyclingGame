using UnityEngine;
using TMPro;
using Cycling.Input;

namespace Cycling.UI
{
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] GameObject pausePanel;
        [SerializeField] TextMeshProUGUI pauseText;

        bool _paused;

        void Start()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnPause += TogglePause;

            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnPause -= TogglePause;

            // Ensure time is restored if destroyed while paused
            Time.timeScale = 1f;
        }

        void TogglePause()
        {
            _paused = !_paused;
            Time.timeScale = _paused ? 0f : 1f;

            if (pausePanel != null)
                pausePanel.SetActive(_paused);
        }
    }
}
