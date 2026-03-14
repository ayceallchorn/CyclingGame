using UnityEngine;
using Cycling.Core;

namespace Cycling.Data
{
    [CreateAssetMenu(fileName = "Bike", menuName = "Cycling/Bike")]
    public class BikeData : ScriptableObject
    {
        public string bikeName = "Default";
        [Tooltip("Bike mass in kg")]
        public float mass = 8f;
        public float cdA = Constants.DefaultCdA;
        public float crr = Constants.DefaultCrr;
        public GearTableData gearTable;
    }
}
