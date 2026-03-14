using UnityEngine;
using UnityEngine.UI;
using Cycling.Cycling;
using Cycling.Race;

namespace Cycling.UI
{
    public class LapProgressBar : MonoBehaviour
    {
        [SerializeField] RiderMotor riderMotor;
        [SerializeField] Image fillImage;

        void Update()
        {
            if (riderMotor == null || fillImage == null) return;

            float trackLen = riderMotor.TrackLength;
            if (trackLen <= 0f) return;

            float progress = riderMotor.DistanceAlongSpline / trackLen;
            fillImage.fillAmount = progress;
        }
    }
}
