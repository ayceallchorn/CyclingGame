using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cycling.Cycling;
using Cycling.Race;

namespace Cycling.UI
{
    public class RaceLeaderboard : MonoBehaviour
    {
        [SerializeField] int visibleRows = 10;

        PositionTracker _positionTracker;
        RiderMotor _playerMotor;
        RectTransform _rowContainer;
        LeaderboardRow[] _rows;
        bool _built;

        struct LeaderboardRow
        {
            public GameObject Root;
            public Image Background;
            public TextMeshProUGUI PositionText;
            public Image TeamDot;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI GapText;
        }

        static readonly Color PlayerHighlight = new(1f, 0.84f, 0f, 0.25f);
        static readonly Color TransparentBg = new(0f, 0f, 0f, 0f);

        void Update()
        {
            // Lazy-init: wait until PositionTracker exists and has riders
            if (!_built)
            {
                if (_positionTracker == null)
                    _positionTracker = Object.FindFirstObjectByType<PositionTracker>();
                if (_positionTracker == null || _positionTracker.SortedRiders.Count == 0) return;

                if (_playerMotor == null)
                {
                    foreach (var id in Object.FindObjectsByType<RiderIdentity>(FindObjectsSortMode.None))
                    {
                        if (id.IsPlayer)
                        {
                            _playerMotor = id.GetComponent<RiderMotor>();
                            break;
                        }
                    }
                }
                if (_playerMotor == null) return;

                BuildContainer();
                BuildRows();
                _built = true;
            }

            UpdateRows();
        }

        void BuildContainer()
        {
            // Resize our own panel to fit content
            var panelRT = GetComponent<RectTransform>();
            if (panelRT != null)
                panelRT.sizeDelta = new Vector2(260, 24 * visibleRows + 8);

            var go = new GameObject("RowContainer", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 1;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            _rowContainer = rt;
        }

        void BuildRows()
        {
            _rows = new LeaderboardRow[visibleRows];

            for (int i = 0; i < visibleRows; i++)
            {
                var rowGO = new GameObject($"Row_{i}", typeof(RectTransform));
                rowGO.transform.SetParent(_rowContainer, false);

                var rowRect = rowGO.GetComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(0, 22);

                var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.spacing = 3;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.padding = new RectOffset(4, 4, 0, 0);
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;

                var bg = rowGO.AddComponent<Image>();
                bg.color = TransparentBg;

                var row = new LeaderboardRow { Root = rowGO, Background = bg };

                // Position number — compact
                row.PositionText = CreateText(rowGO.transform, "Pos", 22f, TextAlignmentOptions.Right, 12);

                // Team color dot — small bar
                var dotGO = new GameObject("Dot", typeof(RectTransform));
                dotGO.transform.SetParent(rowGO.transform, false);
                row.TeamDot = dotGO.AddComponent<Image>();
                var dotLayout = dotGO.AddComponent<LayoutElement>();
                dotLayout.preferredWidth = 4;
                dotLayout.minWidth = 4;

                // Name — takes remaining space
                row.NameText = CreateText(rowGO.transform, "Name", -1f, TextAlignmentOptions.Left, 12);
                var nameLayout = row.NameText.gameObject.AddComponent<LayoutElement>();
                nameLayout.flexibleWidth = 1;
                nameLayout.minWidth = 80;

                // Gap — compact
                row.GapText = CreateText(rowGO.transform, "Gap", 48f, TextAlignmentOptions.Right, 11);

                _rows[i] = row;
            }
        }

        TextMeshProUGUI CreateText(Transform parent, string name, float width, TextAlignmentOptions align, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.alignment = align;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Truncate;

            if (width > 0)
            {
                var le = go.AddComponent<LayoutElement>();
                le.preferredWidth = width;
                le.minWidth = width;
            }

            return tmp;
        }

        void UpdateRows()
        {
            var sorted = _positionTracker.SortedRiders;
            if (sorted.Count == 0) return;

            int playerIdx = -1;
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i] == _playerMotor) { playerIdx = i; break; }
            }
            if (playerIdx < 0) return;

            int half = visibleRows / 2;
            int startIdx = Mathf.Clamp(playerIdx - half, 0, Mathf.Max(0, sorted.Count - visibleRows));
            int endIdx = Mathf.Min(startIdx + visibleRows, sorted.Count);

            float playerEff = _positionTracker.GetEffectiveDistance(_playerMotor);
            float playerSpeed = Mathf.Max(_playerMotor.SpeedMs, 5f);

            for (int r = 0; r < visibleRows; r++)
            {
                int si = startIdx + r;
                if (si >= endIdx)
                {
                    _rows[r].Root.SetActive(false);
                    continue;
                }

                _rows[r].Root.SetActive(true);
                var rider = sorted[si];
                bool isPlayer = rider == _playerMotor;

                _rows[r].PositionText.text = (si + 1).ToString();

                var identity = rider.GetComponent<RiderIdentity>();
                _rows[r].TeamDot.color = identity != null ? identity.TeamColor : Color.grey;
                _rows[r].NameText.text = isPlayer ? "YOU" : (identity != null ? ShortenName(identity.RiderName) : "???");

                if (isPlayer)
                {
                    _rows[r].GapText.text = "";
                    _rows[r].Background.color = PlayerHighlight;
                    _rows[r].NameText.fontStyle = FontStyles.Bold;
                    _rows[r].PositionText.fontStyle = FontStyles.Bold;
                }
                else
                {
                    float riderEff = _positionTracker.GetEffectiveDistance(rider);
                    float gapSeconds = (riderEff - playerEff) / playerSpeed;
                    _rows[r].GapText.text = FormatGap(gapSeconds);
                    _rows[r].Background.color = TransparentBg;
                    _rows[r].NameText.fontStyle = FontStyles.Normal;
                    _rows[r].PositionText.fontStyle = FontStyles.Normal;
                }
            }
        }

        static string ShortenName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "???";
            int spaceIdx = fullName.LastIndexOf(' ');
            if (spaceIdx <= 0) return fullName.ToUpperInvariant();
            char initial = fullName[0];
            string surname = fullName.Substring(spaceIdx + 1).ToUpperInvariant();
            return $"{initial}. {surname}";
        }

        static string FormatGap(float seconds)
        {
            float abs = Mathf.Abs(seconds);
            if (abs < 0.5f) return "";
            string sign = seconds > 0 ? "+" : "-";
            if (abs >= 60f)
            {
                int mins = (int)(abs / 60f);
                int secs = (int)(abs % 60f);
                return $"{sign}{mins}:{secs:D2}";
            }
            return $"{sign}{abs:F0}s";
        }
    }
}
