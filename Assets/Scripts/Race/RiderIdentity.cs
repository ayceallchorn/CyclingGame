using UnityEngine;
using Cycling.Data;

namespace Cycling.Race
{
    public class RiderIdentity : MonoBehaviour
    {
        [SerializeField] RiderData riderData;
        [SerializeField] bool isPlayer;

        public RiderData Data => riderData;
        public bool IsPlayer => isPlayer;
        public string RiderName => riderData != null ? riderData.riderName : "Unknown";
        public TeamData Team => riderData != null ? riderData.team : null;
        public float FTP => riderData != null ? riderData.ftp : 200f;
        public float Weight => riderData != null ? riderData.weight : 72f;

        public void Init(RiderData data, bool player)
        {
            riderData = data;
            isPlayer = player;

            // Apply team colour to renderer
            if (data?.team != null)
            {
                var rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = rend.material;
                    mat.color = data.team.primaryColor;
                }
            }
        }
    }
}
