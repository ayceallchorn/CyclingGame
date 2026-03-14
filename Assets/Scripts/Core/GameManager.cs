using UnityEngine;
using UnityEngine.SceneManagement;
using Cycling.Data;

namespace Cycling.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Race Configuration")]
        public TrackDefinition selectedTrack;
        public int laps = 3;
        public int aiCount = 20;
        public float difficulty = 0.5f;

        [Header("Available Tracks")]
        public TrackDefinition[] tracks;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartRace()
        {
            if (selectedTrack == null && tracks != null && tracks.Length > 0)
                selectedTrack = tracks[0];

            string sceneName = selectedTrack != null ? selectedTrack.sceneName : "RaceScene";
            SceneManager.LoadScene(sceneName);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
