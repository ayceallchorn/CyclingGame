using UnityEngine;

namespace Cycling.Data
{
    [CreateAssetMenu(fileName = "Rider", menuName = "Cycling/Rider")]
    public class RiderData : ScriptableObject
    {
        public string riderName;
        public TeamData team;
        [Tooltip("Functional Threshold Power in watts")]
        public float ftp = 250f;
        [Tooltip("Rider weight in kg (bike weight added separately)")]
        public float weight = 72f;
    }
}
