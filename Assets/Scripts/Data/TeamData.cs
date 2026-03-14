using UnityEngine;

namespace Cycling.Data
{
    [CreateAssetMenu(fileName = "Team", menuName = "Cycling/Team")]
    public class TeamData : ScriptableObject
    {
        public string teamName;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.black;
    }
}
