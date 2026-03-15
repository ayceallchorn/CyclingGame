using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cycling.Data
{
    [Serializable]
    public class RiderDatabaseJson
    {
        public TeamEntry[] teams;
        public RiderEntry[] riders;
    }

    [Serializable]
    public class TeamEntry
    {
        public string id;
        public string name;
        public string primaryColor;
        public string secondaryColor;
    }

    [Serializable]
    public class RiderEntry
    {
        public string name;
        public string team;
        public float ftp;
        public float weight;
        public int priority; // 1-5
    }

    /// <summary>
    /// Loads rider/team data from JSON and provides weighted random selection.
    /// </summary>
    public static class RiderDatabase
    {
        static RiderDatabaseJson _data;
        static Dictionary<string, TeamEntry> _teamLookup;

        public static RiderDatabaseJson Data => _data;

        public static void Load()
        {
            var textAsset = Resources.Load<TextAsset>("RiderDatabase");
            if (textAsset == null)
            {
                Debug.LogError("[RiderDatabase] RiderDatabase.json not found in Resources/");
                return;
            }
            _data = JsonUtility.FromJson<RiderDatabaseJson>(textAsset.text);

            _teamLookup = new Dictionary<string, TeamEntry>();
            if (_data.teams != null)
            {
                foreach (var team in _data.teams)
                    _teamLookup[team.id] = team;
            }

            Debug.Log($"[RiderDatabase] Loaded {_data.riders?.Length ?? 0} riders, {_data.teams?.Length ?? 0} teams.");
        }

        public static TeamEntry GetTeam(string id)
        {
            if (_teamLookup == null) Load();
            return _teamLookup != null && _teamLookup.TryGetValue(id, out var t) ? t : null;
        }

        public static Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.white;
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color c)) return c;
            return Color.white;
        }

        /// <summary>
        /// Select N riders using weighted random (priority = weight).
        /// Ensures team diversity: max maxPerTeam riders from any one team.
        /// </summary>
        public static List<RiderEntry> SelectWeighted(int count, int maxPerTeam = 3)
        {
            if (_data == null) Load();
            if (_data?.riders == null || _data.riders.Length == 0) return new List<RiderEntry>();

            var pool = new List<RiderEntry>(_data.riders);
            var selected = new List<RiderEntry>();
            var teamCounts = new Dictionary<string, int>();

            // Build weighted list
            while (selected.Count < count && pool.Count > 0)
            {
                // Calculate total weight
                float totalWeight = 0f;
                foreach (var r in pool)
                {
                    // Check team cap
                    int teamCount = teamCounts.TryGetValue(r.team, out int tc) ? tc : 0;
                    if (teamCount >= maxPerTeam) continue;
                    totalWeight += r.priority;
                }

                if (totalWeight <= 0f) break;

                float roll = UnityEngine.Random.Range(0f, totalWeight);
                float cumulative = 0f;
                RiderEntry picked = null;

                foreach (var r in pool)
                {
                    int teamCount = teamCounts.TryGetValue(r.team, out int tc) ? tc : 0;
                    if (teamCount >= maxPerTeam) continue;

                    cumulative += r.priority;
                    if (roll <= cumulative)
                    {
                        picked = r;
                        break;
                    }
                }

                if (picked == null) break;

                selected.Add(picked);
                pool.Remove(picked);

                if (!teamCounts.ContainsKey(picked.team))
                    teamCounts[picked.team] = 0;
                teamCounts[picked.team]++;
            }

            return selected;
        }
    }
}
