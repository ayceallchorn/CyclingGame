using UnityEngine;

namespace Cycling.Data
{
    [CreateAssetMenu(fileName = "GearTable", menuName = "Cycling/Gear Table")]
    public class GearTableData : ScriptableObject
    {
        [Tooltip("Chainring teeth count (e.g. 50/34 compact)")]
        public int[] chainringTeeth = { 34, 50 };

        [Tooltip("Cassette sprocket teeth from largest to smallest (easiest to hardest)")]
        public int[] cassetteTeeth = { 28, 25, 23, 21, 19, 17, 15, 14, 13, 12, 11 };

        [Tooltip("Wheel circumference in metres (700x25c ≈ 2.1m)")]
        public float wheelCircumference = 2.1f;

        /// <summary>
        /// Total number of virtual gear combinations (chainring × cassette).
        /// For virtual shifting we flatten into a sorted ratio list.
        /// </summary>
        public int GearCount => chainringTeeth.Length * cassetteTeeth.Length;

        /// <summary>
        /// Returns the gear ratio for a given flat index (0 = easiest, GearCount-1 = hardest).
        /// Ratios are sorted ascending at runtime.
        /// </summary>
        public float GetRatio(int index)
        {
            float[] ratios = GetSortedRatios();
            index = Mathf.Clamp(index, 0, ratios.Length - 1);
            return ratios[index];
        }

        /// <summary>
        /// Returns all gear ratios sorted from easiest (lowest) to hardest (highest).
        /// </summary>
        public float[] GetSortedRatios()
        {
            float[] ratios = new float[chainringTeeth.Length * cassetteTeeth.Length];
            int idx = 0;
            foreach (int chainring in chainringTeeth)
            {
                foreach (int cassette in cassetteTeeth)
                {
                    ratios[idx++] = (float)chainring / cassette;
                }
            }
            System.Array.Sort(ratios);
            return ratios;
        }
    }
}
