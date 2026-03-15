using UnityEngine;
using Cycling.Data;

namespace Cycling.Race
{
    public class RiderIdentity : MonoBehaviour
    {
        [SerializeField] RiderData riderData;
        [SerializeField] bool isPlayer;

        // JSON-based data (used when loaded from RiderDatabase)
        string _riderName;
        string _teamId;
        float _ftp;
        float _weight;
        Color _teamColor = Color.grey;
        string _teamName;

        public bool IsPlayer => isPlayer;
        public string RiderName => !string.IsNullOrEmpty(_riderName) ? _riderName : (riderData != null ? riderData.riderName : "Unknown");
        public float FTP => _ftp > 0 ? _ftp : (riderData != null ? riderData.ftp : 200f);
        public float Weight => _weight > 0 ? _weight : (riderData != null ? riderData.weight : 72f);
        public Color TeamColor => _teamColor;
        public string TeamName => _teamName ?? (riderData?.team != null ? riderData.team.teamName : "Unknown");

        // Legacy support
        public RiderData Data => riderData;
        public TeamData Team => riderData != null ? riderData.team : null;

        /// <summary>
        /// Init from JSON rider entry.
        /// </summary>
        public void InitFromJson(RiderEntry entry, bool player)
        {
            isPlayer = player;
            _riderName = entry.name;
            _teamId = entry.team;
            _ftp = entry.ftp;
            _weight = entry.weight;

            var teamEntry = RiderDatabase.GetTeam(entry.team);
            if (teamEntry != null)
            {
                _teamColor = RiderDatabase.ParseColor(teamEntry.primaryColor);
                _teamName = teamEntry.name;
            }
        }

        /// <summary>
        /// Legacy init from ScriptableObject.
        /// </summary>
        public void Init(RiderData data, bool player)
        {
            riderData = data;
            isPlayer = player;
            if (data != null)
            {
                _riderName = data.riderName;
                _ftp = data.ftp;
                _weight = data.weight;
            }
            if (data?.team != null)
            {
                _teamColor = data.team.primaryColor;
                _teamName = data.team.teamName;
            }
        }
    }
}
