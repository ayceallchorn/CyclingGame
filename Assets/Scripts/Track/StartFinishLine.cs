using UnityEngine;

namespace Cycling.Track
{
    /// <summary>
    /// Visual start/finish line marker. Place at the spline origin.
    /// </summary>
    public class StartFinishLine : MonoBehaviour
    {
        [SerializeField] TrackSpline trackSpline;
        [SerializeField] float lineWidth = 8f;
        [SerializeField] float lineDepth = 2f;
        [SerializeField] int checkerCount = 8;

        public void Generate()
        {
            if (trackSpline == null) return;

            // Position at spline start
            trackSpline.Evaluate(0f, out Vector3 pos, out Quaternion rot);
            transform.position = pos + Vector3.up * 0.1f;
            transform.rotation = rot;

            // Create checkerboard texture
            int texSize = checkerCount * 2;
            var tex = new Texture2D(texSize, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int x = 0; x < texSize; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    bool white = (x + y) % 2 == 0;
                    tex.SetPixel(x, y, white ? Color.white : Color.black);
                }
            }
            tex.Apply();

            // Create quad mesh
            var mf = GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.name = "StartFinishLine";
            float hw = lineWidth * 0.5f;
            float hd = lineDepth * 0.5f;
            mesh.vertices = new Vector3[]
            {
                new Vector3(-hw, 0, -hd),
                new Vector3(hw, 0, -hd),
                new Vector3(-hw, 0, hd),
                new Vector3(hw, 0, hd)
            };
            mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 1), new Vector2(1, 1)
            };
            mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mf.mesh = mesh;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.mainTexture = tex;
            mr.material = mat;
        }
    }
}
