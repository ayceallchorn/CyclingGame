using UnityEngine;
using Cycling.Cycling;
using Cycling.Input;

namespace Cycling.Audio
{
    public class RaceAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] RiderMotor playerMotor;

        [Header("Wind")]
        [SerializeField] AudioSource windSource;
        [SerializeField] float windMinSpeed = 5f;    // m/s where wind starts
        [SerializeField] float windMaxSpeed = 20f;    // m/s for full volume
        [SerializeField] float windMinPitch = 0.6f;
        [SerializeField] float windMaxPitch = 1.4f;

        [Header("Gear Click")]
        [SerializeField] AudioSource gearClickSource;

        void Start()
        {
            // Generate procedural wind sound (white noise filtered)
            if (windSource != null && windSource.clip == null)
                windSource.clip = GenerateWhiteNoise(2f);

            if (windSource != null)
            {
                windSource.loop = true;
                windSource.volume = 0f;
                windSource.Play();
            }

            // Generate gear click (short blip)
            if (gearClickSource != null && gearClickSource.clip == null)
                gearClickSource.clip = GenerateClick();

            // Subscribe to gear shifts
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnShiftUp += PlayGearClick;
                InputManager.Instance.OnShiftDown += PlayGearClick;
            }
        }

        void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnShiftUp -= PlayGearClick;
                InputManager.Instance.OnShiftDown -= PlayGearClick;
            }
        }

        void Update()
        {
            if (windSource == null || playerMotor == null) return;

            float speed = playerMotor.SpeedMs;
            float t = Mathf.InverseLerp(windMinSpeed, windMaxSpeed, speed);
            windSource.volume = Mathf.Lerp(0f, 0.3f, t);
            windSource.pitch = Mathf.Lerp(windMinPitch, windMaxPitch, t);
        }

        void PlayGearClick()
        {
            if (gearClickSource != null)
                gearClickSource.PlayOneShot(gearClickSource.clip, 0.5f);
        }

        static AudioClip GenerateWhiteNoise(float duration)
        {
            int sampleRate = 22050;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            // Low-pass filtered white noise for wind effect
            float prev = 0f;
            float alpha = 0.15f;
            for (int i = 0; i < samples; i++)
            {
                float noise = Random.Range(-1f, 1f);
                prev = alpha * noise + (1f - alpha) * prev;
                data[i] = prev;
            }

            var clip = AudioClip.Create("Wind", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip GenerateClick()
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.05f); // 50ms click
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float envelope = 1f - t; // quick decay
                data[i] = Mathf.Sin(i * 0.3f) * envelope * 0.6f; // metallic click
            }

            var clip = AudioClip.Create("GearClick", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
