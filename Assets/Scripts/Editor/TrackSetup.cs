using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Cycling.Editor
{
    public static class TrackSetup
    {
        [MenuItem("Cycling/Setup Track Spline")]
        public static void SetupTrackSpline()
        {
            var trackGO = GameObject.Find("Track");
            if (trackGO == null)
            {
                Debug.LogError("No 'Track' GameObject found in scene.");
                return;
            }

            var container = trackGO.GetComponent<SplineContainer>();
            if (container == null)
            {
                Debug.LogError("No SplineContainer on Track.");
                return;
            }

            // Clear existing splines
            while (container.Splines.Count > 0)
                container.RemoveSplineAt(0);

            // Create an oval track ~200m, with a gentle hill on one side
            // The track is roughly 80m x 30m oval
            var spline = container.AddSpline();
            spline.Closed = true;

            // Control points going clockwise when viewed from above
            // Using BezierKnot with tangent handles for smooth curves
            float r = 40f; // half-length
            float w = 15f; // half-width
            float hillHeight = 5f;

            var knots = new BezierKnot[]
            {
                // Bottom straight (start/finish) - flat
                new BezierKnot(
                    new float3(0, 0, -w),
                    new float3(-r * 0.55f, 0, 0),
                    new float3(r * 0.55f, 0, 0)),

                // Right curve
                new BezierKnot(
                    new float3(r, 0, 0),
                    new float3(0, 0, -w * 0.55f),
                    new float3(0, 0, w * 0.55f)),

                // Top straight (back stretch) - hill
                new BezierKnot(
                    new float3(0, hillHeight, w),
                    new float3(r * 0.55f, 0, 0),
                    new float3(-r * 0.55f, 0, 0)),

                // Left curve
                new BezierKnot(
                    new float3(-r, 0, 0),
                    new float3(0, 0, w * 0.55f),
                    new float3(0, 0, -w * 0.55f)),
            };

            foreach (var knot in knots)
                spline.Add(knot, TangentMode.Mirrored);

            EditorUtility.SetDirty(container);
            Debug.Log($"Track spline created: {spline.Count} knots, closed={spline.Closed}, length≈{container.CalculateLength():F1}m");
        }
    }
}
