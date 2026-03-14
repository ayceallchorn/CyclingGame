using UnityEngine;

namespace Cycling.Data
{
    [CreateAssetMenu(fileName = "Track", menuName = "Cycling/Track Definition")]
    public class TrackDefinition : ScriptableObject
    {
        public string trackName = "Circuit One";
        public string sceneName = "RaceScene";
        [Tooltip("Approximate track length in metres")]
        public float length = 182f;
        public Sprite thumbnail;
    }
}
