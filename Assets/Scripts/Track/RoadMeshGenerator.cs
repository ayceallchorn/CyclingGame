using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Cycling.Track
{
    [RequireComponent(typeof(SplineContainer), typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadMeshGenerator : MonoBehaviour
    {
        [Header("Road Settings")]
        [SerializeField] float roadWidth = 8f;
        [SerializeField] int samplesPerMeter = 2;

        SplineContainer _container;

        public float RoadWidth => roadWidth;

        public void Generate()
        {
            _container = GetComponent<SplineContainer>();
            if (_container == null || _container.Splines.Count == 0)
            {
                Debug.LogError("[RoadMeshGenerator] No SplineContainer or splines found.");
                return;
            }

            float totalLength = _container.CalculateLength();
            if (totalLength < 1f)
            {
                Debug.LogError($"[RoadMeshGenerator] Spline too short: {totalLength}m");
                return;
            }

            int sampleCount = Mathf.Max(4, Mathf.RoundToInt(totalLength * samplesPerMeter));
            int vertCount = (sampleCount + 1) * 2;
            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            int triCount = sampleCount * 6;
            var triangles = new int[triCount];

            float halfWidth = roadWidth * 0.5f;

            for (int i = 0; i <= sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                _container.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);

                Vector3 p = (Vector3)pos;
                Vector3 fwd = math.lengthsq(tangent) > 0.0001f
                    ? ((Vector3)math.normalize(tangent))
                    : Vector3.forward;

                // Use world up for a flat road feel, but respect spline Y
                Vector3 right = Vector3.Cross(fwd, Vector3.up).normalized;
                if (right.sqrMagnitude < 0.001f)
                    right = Vector3.Cross(fwd, Vector3.forward).normalized;

                int baseIdx = i * 2;
                vertices[baseIdx] = p - right * halfWidth + Vector3.up * 0.15f;
                vertices[baseIdx + 1] = p + right * halfWidth + Vector3.up * 0.15f;

                float uvY = t * totalLength / 10f;
                uvs[baseIdx] = new Vector2(0f, uvY);
                uvs[baseIdx + 1] = new Vector2(1f, uvY);
            }

            int ti = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                int bl = i * 2;
                int br = bl + 1;
                int tl = bl + 2;
                int tr = bl + 3;

                triangles[ti++] = bl;
                triangles[ti++] = br;
                triangles[ti++] = tl;
                triangles[ti++] = br;
                triangles[ti++] = tr;
                triangles[ti++] = tl;
            }

            var mesh = new Mesh();
            mesh.name = "RoadMesh";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = mesh;
            Debug.Log($"[RoadMeshGenerator] Generated road: {sampleCount} segments, {totalLength:F0}m, {roadWidth}m wide");
        }

#if UNITY_EDITOR
        [ContextMenu("Regenerate Road Mesh")]
        void RegenerateInEditor()
        {
            Generate();
        }
#endif
    }
}
