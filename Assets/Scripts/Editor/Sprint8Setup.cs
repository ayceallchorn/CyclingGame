using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using TMPro;

namespace Cycling.Editor
{
    public static class Sprint8Setup
    {
        [MenuItem("Cycling/Setup Sprint 8 Polish")]
        public static void Setup()
        {
            SetupRoadMesh();
            SetupAudio();
            SetupLapProgressBar();
            SetupEnvironment();
            Debug.Log("Sprint 8 polish applied: road mesh, audio, progress bar, environment.");
        }

        static void SetupRoadMesh()
        {
            var track = GameObject.Find("Track");
            if (track == null) return;

            // Add SplineExtrude for road surface
            var extrude = track.GetComponent<SplineExtrude>();
            if (extrude == null)
                extrude = track.AddComponent<SplineExtrude>();

            // Configure road width and segments
            var so = new SerializedObject(extrude);
            // Rebuild on change
            so.FindProperty("m_RebuildOnSplineChange").boolValue = true;
            so.FindProperty("m_SegmentsPerUnit").intValue = 4;
            so.FindProperty("m_Sides").intValue = 2;
            so.FindProperty("m_Radius").floatValue = 3f; // 6m wide road
            so.ApplyModifiedProperties();

            // Add a road material
            var roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMat.color = new Color(0.25f, 0.25f, 0.27f); // dark grey asphalt
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.CreateAsset(roadMat, "Assets/Materials/Road.mat");

            var renderer = track.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = track.AddComponent<MeshRenderer>();
            renderer.material = roadMat;
        }

        static void SetupAudio()
        {
            var audioGO = new GameObject("Race Audio");

            // Wind source
            var windSource = audioGO.AddComponent<AudioSource>();
            windSource.spatialBlend = 0f; // 2D sound
            windSource.loop = true;
            windSource.playOnAwake = false;

            // Gear click source
            var gearSource = audioGO.AddComponent<AudioSource>();
            gearSource.spatialBlend = 0f;
            gearSource.playOnAwake = false;

            // Add RaceAudio component
            var raceAudio = audioGO.AddComponent<Audio.RaceAudio>();

            var playerMotor = GameObject.Find("Rider")?.GetComponent<Cycling.RiderMotor>();

            // Wire via SerializedObject
            var so = new SerializedObject(raceAudio);
            if (playerMotor != null)
                so.FindProperty("playerMotor").objectReferenceValue = playerMotor;

            // Get audio sources (first = wind, second = gear)
            var sources = audioGO.GetComponents<AudioSource>();
            so.FindProperty("windSource").objectReferenceValue = sources[0];
            so.FindProperty("gearClickSource").objectReferenceValue = sources[1];
            so.ApplyModifiedProperties();
        }

        static void SetupLapProgressBar()
        {
            var hudCanvas = GameObject.Find("HUD Canvas");
            if (hudCanvas == null) return;

            // Create progress bar at top of screen
            var barBG = new GameObject("Lap Progress BG", typeof(RectTransform));
            barBG.transform.SetParent(hudCanvas.transform, false);
            var bgRT = barBG.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 1);
            bgRT.anchorMax = new Vector2(1, 1);
            bgRT.pivot = new Vector2(0.5f, 1);
            bgRT.anchoredPosition = new Vector2(0, 0);
            bgRT.sizeDelta = new Vector2(0, 8);
            var bgImg = barBG.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

            var barFill = new GameObject("Lap Progress Fill", typeof(RectTransform));
            barFill.transform.SetParent(barBG.transform, false);
            var fillRT = barFill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            var fillImg = barFill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.8f, 0.3f, 0.9f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0f;

            // Add LapProgressBar component
            var progressBar = barBG.AddComponent<UI.LapProgressBar>();
            var so = new SerializedObject(progressBar);
            var playerMotor = Object.FindFirstObjectByType<Cycling.RiderMotor>();
            if (playerMotor != null)
                so.FindProperty("riderMotor").objectReferenceValue = playerMotor;
            so.FindProperty("fillImage").objectReferenceValue = fillImg;
            so.ApplyModifiedProperties();
        }

        static void SetupEnvironment()
        {
            // Create environment parent
            var envGO = new GameObject("Environment");

            // Improve ground — make it green
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                var grassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                grassMat.color = new Color(0.25f, 0.55f, 0.18f); // grass green
                AssetDatabase.CreateAsset(grassMat, "Assets/Materials/Grass.mat");
                ground.GetComponent<MeshRenderer>().material = grassMat;
            }

            // Place some simple trees (tall cylinders with green spheres)
            var treeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            treeMat.color = new Color(0.15f, 0.45f, 0.12f);
            AssetDatabase.CreateAsset(treeMat, "Assets/Materials/TreeFoliage.mat");

            var trunkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            trunkMat.color = new Color(0.4f, 0.25f, 0.1f);
            AssetDatabase.CreateAsset(trunkMat, "Assets/Materials/TreeTrunk.mat");

            // Place trees around the track
            float[] treeAngles = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330 };
            float radius = 55f;

            foreach (float angle in treeAngles)
            {
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Cos(rad) * radius;
                float z = Mathf.Sin(rad) * radius;

                var tree = new GameObject($"Tree_{angle}");
                tree.transform.SetParent(envGO.transform);
                tree.transform.position = new Vector3(x, 0, z);

                // Trunk
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(tree.transform, false);
                trunk.transform.localPosition = new Vector3(0, 2f, 0);
                trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
                trunk.GetComponent<MeshRenderer>().material = trunkMat;
                Object.DestroyImmediate(trunk.GetComponent<Collider>());

                // Foliage
                var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.name = "Foliage";
                foliage.transform.SetParent(tree.transform, false);
                foliage.transform.localPosition = new Vector3(0, 5f, 0);
                foliage.transform.localScale = new Vector3(3f, 3.5f, 3f);
                foliage.GetComponent<MeshRenderer>().material = treeMat;
                Object.DestroyImmediate(foliage.GetComponent<Collider>());

                // Slight random variation
                float scaleVar = Random.Range(0.7f, 1.3f);
                tree.transform.localScale = Vector3.one * scaleVar;
            }

            // Place some inner trees too
            float innerRadius = 25f;
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f + 15f;
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Cos(rad) * innerRadius;
                float z = Mathf.Sin(rad) * innerRadius;

                var tree = new GameObject($"InnerTree_{i}");
                tree.transform.SetParent(envGO.transform);
                tree.transform.position = new Vector3(x, 0, z);

                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(tree.transform, false);
                trunk.transform.localPosition = new Vector3(0, 2f, 0);
                trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
                trunk.GetComponent<MeshRenderer>().material = trunkMat;
                Object.DestroyImmediate(trunk.GetComponent<Collider>());

                var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.name = "Foliage";
                foliage.transform.SetParent(tree.transform, false);
                foliage.transform.localPosition = new Vector3(0, 5f, 0);
                foliage.transform.localScale = new Vector3(3f, 3.5f, 3f);
                foliage.GetComponent<MeshRenderer>().material = treeMat;
                Object.DestroyImmediate(foliage.GetComponent<Collider>());

                float scaleVar = Random.Range(0.7f, 1.3f);
                tree.transform.localScale = Vector3.one * scaleVar;
            }

            AssetDatabase.SaveAssets();
        }
    }
}
