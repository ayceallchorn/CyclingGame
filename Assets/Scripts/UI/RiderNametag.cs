using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Cycling.UI
{
    /// <summary>
    /// Floating nametag above a rider. Billboards to camera.
    /// </summary>
    public class RiderNametag : MonoBehaviour
    {
        [SerializeField] float heightOffset = 2.5f;
        [SerializeField] float maxDistance = 60f;
        [SerializeField] float minScale = 0.3f;

        Transform _target;
        Canvas _canvas;
        TextMeshProUGUI _nameText;
        Image _background;
        UnityEngine.Camera _mainCam;

        public void Init(Transform target, string riderName, Color teamColor)
        {
            _target = target;

            // Create world-space canvas
            var canvasGO = new GameObject("NametagCanvas");
            canvasGO.transform.SetParent(transform, false);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 5;

            var rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 22);
            rt.localScale = Vector3.one * 0.006f; // scale down for world space

            // Background
            var bgGO = new GameObject("BG", typeof(RectTransform));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            _background = bgGO.AddComponent<Image>();
            _background.color = new Color(teamColor.r, teamColor.g, teamColor.b, 1f);

            // Name text
            var textGO = new GameObject("Name", typeof(RectTransform));
            textGO.transform.SetParent(canvasGO.transform, false);
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(5, 0);
            textRT.offsetMax = new Vector2(-5, 0);
            _nameText = textGO.AddComponent<TextMeshProUGUI>();
            _nameText.text = riderName;
            _nameText.fontSize = 14;
            _nameText.alignment = TextAlignmentOptions.Center;

            // Choose text color based on background brightness
            float brightness = teamColor.r * 0.299f + teamColor.g * 0.587f + teamColor.b * 0.114f;
            _nameText.color = brightness > 0.5f ? Color.black : Color.white;
        }

        void LateUpdate()
        {
            if (_target == null || _canvas == null) return;

            if (_mainCam == null)
                _mainCam = UnityEngine.Camera.main;
            if (_mainCam == null) return;

            // Position above rider
            transform.position = _target.position + Vector3.up * heightOffset;

            // Billboard to camera
            transform.rotation = Quaternion.LookRotation(
                transform.position - _mainCam.transform.position, Vector3.up);

            // Scale based on distance (closer = bigger, but capped)
            float dist = Vector3.Distance(transform.position, _mainCam.transform.position);
            float t = Mathf.Clamp01(dist / maxDistance);
            float scale = Mathf.Lerp(1f, minScale, t);
            transform.localScale = Vector3.one * scale;

            // Fade out at distance
            float alpha = Mathf.Lerp(1f, 0f, t);
            if (_background != null)
            {
                var c = _background.color;
                c.a = alpha;
                _background.color = c;
            }
            if (_nameText != null)
            {
                var c = _nameText.color;
                c.a = alpha;
                _nameText.color = c;
            }
        }
    }
}
