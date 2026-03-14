using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Unity.Mathematics;
using TMPro;

namespace Cycling.Editor
{
    public static class TrackSceneBuilder
    {
        [MenuItem("Cycling/Build New Track Scene")]
        public static void BuildTrackScene()
        {
            EditorSceneManager.SaveOpenScenes();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Lighting ===
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.7f);

            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.96f, 0.88f);
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

            // === Track Spline ===
            var trackGO = new GameObject("Track");
            var container = trackGO.AddComponent<SplineContainer>();
            trackGO.AddComponent<Track.TrackSpline>();
            trackGO.AddComponent<Track.ElevationSampler>();
            var roadGen = trackGO.AddComponent<Track.RoadMeshGenerator>();
            // MeshFilter and MeshRenderer added by RequireComponent on RoadMeshGenerator
            var trackRenderer = trackGO.GetComponent<MeshRenderer>();

            // Build a ~1.5km circuit with a hill section
            var spline = container.Splines[0];
            spline.Closed = true;

            // Roughly oval: 400m straights, 100m curves, with a hill on the back straight
            // Points clockwise from above
            float straight = 200f;
            float width = 60f;
            float hillHeight = 15f;

            var knots = new BezierKnot[]
            {
                // Start/finish straight (south) - flat
                new BezierKnot(
                    new float3(0, 0, -width),
                    new float3(-straight * 0.55f, 0, 0),
                    new float3(straight * 0.55f, 0, 0)),
                // East curve - slight rise
                new BezierKnot(
                    new float3(straight, 3f, 0),
                    new float3(0, 0, -width * 0.55f),
                    new float3(0, 0, width * 0.55f)),
                // Back straight midpoint (north) - hill peak
                new BezierKnot(
                    new float3(0, hillHeight, width),
                    new float3(straight * 0.55f, 0, 0),
                    new float3(-straight * 0.55f, 0, 0)),
                // West curve - descent
                new BezierKnot(
                    new float3(-straight, 3f, 0),
                    new float3(0, 0, width * 0.55f),
                    new float3(0, 0, -width * 0.55f)),
            };

            foreach (var knot in knots)
                spline.Add(knot, TangentMode.Mirrored);

            // Road material
            var roadMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Road.mat");
            if (roadMat == null)
            {
                roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                roadMat.color = new Color(0.25f, 0.25f, 0.27f);
                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                    AssetDatabase.CreateFolder("Assets", "Materials");
                AssetDatabase.CreateAsset(roadMat, "Assets/Materials/Road.mat");
            }
            trackRenderer.material = roadMat;

            // Generate road mesh
            var roadGenSO = new SerializedObject(roadGen);
            roadGenSO.FindProperty("roadWidth").floatValue = 8f;
            roadGenSO.FindProperty("samplesPerMeter").intValue = 2;
            roadGenSO.ApplyModifiedProperties();
            roadGen.Generate();

            // Wire ElevationSampler
            var elevSampler = trackGO.GetComponent<Track.ElevationSampler>();
            var elevSO = new SerializedObject(elevSampler);
            elevSO.FindProperty("trackSpline").objectReferenceValue = trackGO.GetComponent<Track.TrackSpline>();
            elevSO.ApplyModifiedProperties();

            // === Terrain (follows road elevation) ===
            BuildTerrain(container);

            // === Start/Finish Line ===
            var ts = trackGO.GetComponent<Track.TrackSpline>();
            float sfRoadWidth = 10f;
            float sfArchHeight = 5f;
            var sfGO = new GameObject("StartFinishLine");
            var sfComp = sfGO.AddComponent<Track.StartFinishLine>();
            var sfSO = new SerializedObject(sfComp);
            sfSO.FindProperty("trackSpline").objectReferenceValue = ts;
            sfSO.FindProperty("roadWidth").floatValue = sfRoadWidth;
            sfSO.ApplyModifiedProperties();

            // Position at spline start
            container.Evaluate(0f, out Unity.Mathematics.float3 sfPos, out Unity.Mathematics.float3 sfTan, out Unity.Mathematics.float3 sfUp);
            sfGO.transform.position = (Vector3)sfPos;
            if (Unity.Mathematics.math.lengthsq(sfTan) > 0.0001f)
            {
                Vector3 fwd = Unity.Mathematics.math.normalize(sfTan);
                sfGO.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            }

            BuildStartFinishVisuals(sfGO.transform, sfRoadWidth, sfArchHeight);

            // === Player Rider ===
            var riderGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            riderGO.name = "Rider";
            riderGO.transform.position = new Vector3(0, 1, -width);

            var motor = riderGO.AddComponent<Cycling.RiderMotor>();
            var motorSO = new SerializedObject(motor);
            motorSO.FindProperty("trackSpline").objectReferenceValue = ts;
            motorSO.FindProperty("verticalOffset").floatValue = 1.7f;
            motorSO.ApplyModifiedProperties();

            riderGO.AddComponent<Cycling.GearSystem>();
            var lapTracker = riderGO.AddComponent<Race.LapTracker>();
            var identity = riderGO.AddComponent<Race.RiderIdentity>();
            var identSO = new SerializedObject(identity);
            identSO.FindProperty("isPlayer").boolValue = true;
            identSO.ApplyModifiedProperties();

            // Bike visual
            var bikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/RayznGames/BicycleSystem/URP/Prefabs/WithRIgs/Bicycle_2.0_RIg.prefab");
            if (bikePrefab != null)
            {
                var visual = riderGO.AddComponent<Cycling.RiderVisual>();
                var visSO = new SerializedObject(visual);
                visSO.FindProperty("visualPrefab").objectReferenceValue = bikePrefab;
                visSO.ApplyModifiedProperties();
            }

            // Gear table
            var gearTable = AssetDatabase.LoadAssetAtPath<Data.GearTableData>("Assets/Data/GearTable_Default.asset");
            if (gearTable != null)
            {
                var gearSys = riderGO.GetComponent<Cycling.GearSystem>();
                var gearSO = new SerializedObject(gearSys);
                gearSO.FindProperty("gearTable").objectReferenceValue = gearTable;
                gearSO.ApplyModifiedProperties();
            }

            // === Camera ===
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<UnityEngine.Camera>();
            camGO.AddComponent<UnityEngine.AudioListener>();
            var riderCam = camGO.AddComponent<Camera.RiderCamera>();
            var camSO = new SerializedObject(riderCam);
            camSO.FindProperty("target").objectReferenceValue = riderGO.transform;
            camSO.ApplyModifiedProperties();
            camGO.transform.position = new Vector3(0, 3, -width - 6);

            // === Input Manager ===
            var inputGO = new GameObject("InputManager");
            inputGO.AddComponent<Input.InputManager>();

            // === Event System ===
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();

            // === Race Infrastructure ===
            var draftGO = new GameObject("Drafting System");
            var drafting = draftGO.AddComponent<Cycling.DraftingSystem>();
            var draftSO = new SerializedObject(drafting);
            draftSO.FindProperty("trackSpline").objectReferenceValue = ts;
            draftSO.ApplyModifiedProperties();

            var posGO = new GameObject("Position Tracker");
            var posTracker = posGO.AddComponent<Race.PositionTracker>();
            var posSO = new SerializedObject(posTracker);
            posSO.FindProperty("trackSpline").objectReferenceValue = ts;
            posSO.ApplyModifiedProperties();

            var rmGO = new GameObject("Race Manager");
            var rm = rmGO.AddComponent<Race.RaceManager>();

            // Load rider data
            var riderGuids = AssetDatabase.FindAssets("t:RiderData", new[] { "Assets/Data/Riders" });
            var riderDataList = new Data.RiderData[riderGuids.Length];
            for (int i = 0; i < riderGuids.Length; i++)
                riderDataList[i] = AssetDatabase.LoadAssetAtPath<Data.RiderData>(
                    AssetDatabase.GUIDToAssetPath(riderGuids[i]));

            var defaultStrategy = AssetDatabase.LoadAssetAtPath<AI.AIStrategyData>("Assets/Data/AIStrategy_Default.asset");
            var defaultBike = AssetDatabase.LoadAssetAtPath<Data.BikeData>("Assets/Data/Bike_Default.asset");

            var rmSO = new SerializedObject(rm);
            rmSO.FindProperty("trackSpline").objectReferenceValue = ts;
            rmSO.FindProperty("totalLaps").intValue = 3;
            rmSO.FindProperty("aiRiderCount").intValue = 20;
            rmSO.FindProperty("playerMotor").objectReferenceValue = motor;
            rmSO.FindProperty("draftingSystem").objectReferenceValue = drafting;
            rmSO.FindProperty("positionTracker").objectReferenceValue = posTracker;
            if (defaultStrategy != null)
                rmSO.FindProperty("defaultStrategy").objectReferenceValue = defaultStrategy;
            if (defaultBike != null)
                rmSO.FindProperty("defaultBike").objectReferenceValue = defaultBike;
            if (bikePrefab != null)
                rmSO.FindProperty("riderVisualPrefab").objectReferenceValue = bikePrefab;
            var riderListProp = rmSO.FindProperty("aiRiderDataList");
            riderListProp.arraySize = riderDataList.Length;
            for (int i = 0; i < riderDataList.Length; i++)
                riderListProp.GetArrayElementAtIndex(i).objectReferenceValue = riderDataList[i];
            rmSO.ApplyModifiedProperties();

            // === Trainer Manager ===
            var trainerGO = new GameObject("Trainer Manager");
            var trainer = trainerGO.AddComponent<Bluetooth.TrainerManager>();
            var trainerSO = new SerializedObject(trainer);
            trainerSO.FindProperty("playerMotor").objectReferenceValue = motor;
            trainerSO.ApplyModifiedProperties();

            // === Audio ===
            var audioGO = new GameObject("Race Audio");
            var windSrc = audioGO.AddComponent<AudioSource>();
            windSrc.spatialBlend = 0f;
            windSrc.loop = true;
            windSrc.playOnAwake = false;
            var gearSrc = audioGO.AddComponent<AudioSource>();
            gearSrc.spatialBlend = 0f;
            gearSrc.playOnAwake = false;
            var raceAudio = audioGO.AddComponent<Audio.RaceAudio>();
            var audioSO = new SerializedObject(raceAudio);
            audioSO.FindProperty("playerMotor").objectReferenceValue = motor;
            var sources = audioGO.GetComponents<AudioSource>();
            audioSO.FindProperty("windSource").objectReferenceValue = sources[0];
            audioSO.FindProperty("gearClickSource").objectReferenceValue = sources[1];
            audioSO.ApplyModifiedProperties();

            // === UI (reuse Sprint2/4/6 setup approach but inline) ===
            BuildUI(motor, riderGO.GetComponent<Cycling.GearSystem>(), posTracker);

            // === Trees ===
            BuildEnvironment(container);

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GrandCircuit.unity");

            // Update build settings
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in buildScenes)
                if (s.path == "Assets/Scenes/GrandCircuit.unity") found = true;
            if (!found)
                buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/GrandCircuit.unity", true));
            EditorBuildSettings.scenes = buildScenes.ToArray();

            // Create TrackDefinition
            var trackDef = ScriptableObject.CreateInstance<Data.TrackDefinition>();
            trackDef.trackName = "Grand Circuit";
            trackDef.sceneName = "GrandCircuit";
            trackDef.length = container.CalculateLength();
            if (!AssetDatabase.IsValidFolder("Assets/Data/Tracks"))
                AssetDatabase.CreateFolder("Assets/Data", "Tracks");
            AssetDatabase.CreateAsset(trackDef, "Assets/Data/Tracks/GrandCircuit.asset");
            AssetDatabase.SaveAssets();

            Debug.Log($"Grand Circuit scene built: {trackDef.length:F0}m track with hill. Saved to Assets/Scenes/GrandCircuit.unity");
        }

        static void BuildStartFinishVisuals(Transform parent, float roadWidth, float archHeight)
        {
            float hw = roadWidth * 0.5f;
            float archThickness = 0.3f;

            // --- Ground markings ---
            // Two white lines
            for (int i = 0; i < 2; i++)
            {
                var lineGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
                lineGO.name = i == 0 ? "StartLine" : "FinishLine";
                lineGO.transform.SetParent(parent, false);
                lineGO.transform.localPosition = new Vector3(0, 0.12f, i == 0 ? -0.6f : 0.6f);
                lineGO.transform.localRotation = Quaternion.Euler(90, 0, 0);
                lineGO.transform.localScale = new Vector3(roadWidth, 0.3f, 1f);
                Object.DestroyImmediate(lineGO.GetComponent<Collider>());
                var lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                lineMat.color = Color.white;
                lineGO.GetComponent<MeshRenderer>().material = lineMat;
            }

            // Checkerboard strip between lines
            var checkerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            checkerGO.name = "CheckerStrip";
            checkerGO.transform.SetParent(parent, false);
            checkerGO.transform.localPosition = new Vector3(0, 0.11f, 0);
            checkerGO.transform.localRotation = Quaternion.Euler(90, 0, 0);
            checkerGO.transform.localScale = new Vector3(roadWidth, 1f, 1f);
            Object.DestroyImmediate(checkerGO.GetComponent<Collider>());

            int checks = 16;
            var tex = new Texture2D(checks, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            for (int x = 0; x < checks; x++)
                for (int y = 0; y < 2; y++)
                    tex.SetPixel(x, y, (x + y) % 2 == 0 ? Color.white : new Color(0.1f, 0.1f, 0.1f));
            tex.Apply();
            var checkerMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            checkerMat.mainTexture = tex;
            checkerGO.GetComponent<MeshRenderer>().material = checkerMat;

            // --- Arch ---
            var archGO = new GameObject("Arch");
            archGO.transform.SetParent(parent, false);

            // Pillars
            var pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pillarMat.color = new Color(0.2f, 0.2f, 0.25f);

            var leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPillar.name = "LeftPillar";
            leftPillar.transform.SetParent(archGO.transform, false);
            leftPillar.transform.localPosition = new Vector3(-hw - 0.5f, archHeight * 0.5f, 0);
            leftPillar.transform.localScale = new Vector3(archThickness, archHeight, archThickness);
            Object.DestroyImmediate(leftPillar.GetComponent<Collider>());
            leftPillar.GetComponent<MeshRenderer>().material = pillarMat;

            var rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPillar.name = "RightPillar";
            rightPillar.transform.SetParent(archGO.transform, false);
            rightPillar.transform.localPosition = new Vector3(hw + 0.5f, archHeight * 0.5f, 0);
            rightPillar.transform.localScale = new Vector3(archThickness, archHeight, archThickness);
            Object.DestroyImmediate(rightPillar.GetComponent<Collider>());
            rightPillar.GetComponent<MeshRenderer>().material = pillarMat;

            var topBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topBeam.name = "TopBeam";
            topBeam.transform.SetParent(archGO.transform, false);
            topBeam.transform.localPosition = new Vector3(0, archHeight, 0);
            topBeam.transform.localScale = new Vector3(roadWidth + 1.5f, archThickness * 2f, archThickness);
            Object.DestroyImmediate(topBeam.GetComponent<Collider>());
            topBeam.GetComponent<MeshRenderer>().material = pillarMat;

            // Banner (translucent checkerboard)
            int bw = 16, bh = 4;
            var bannerTex = new Texture2D(bw, bh, TextureFormat.RGBA32, false);
            bannerTex.filterMode = FilterMode.Point;
            for (int x = 0; x < bw; x++)
                for (int y = 0; y < bh; y++)
                    bannerTex.SetPixel(x, y, (x + y) % 2 == 0
                        ? new Color(1f, 1f, 1f, 0.7f)
                        : new Color(0.1f, 0.1f, 0.1f, 0.7f));
            bannerTex.Apply();

            var bannerMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bannerMat.mainTexture = bannerTex;
            bannerMat.SetFloat("_Surface", 1);
            bannerMat.SetFloat("_Blend", 0);
            bannerMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bannerMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bannerMat.SetInt("_ZWrite", 0);
            bannerMat.renderQueue = 3000;
            bannerMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            bannerMat.SetColor("_BaseColor", new Color(1, 1, 1, 0.7f));

            foreach (float yaw in new[] { 0f, 180f })
            {
                var bannerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bannerGO.name = yaw == 0f ? "BannerFront" : "BannerBack";
                bannerGO.transform.SetParent(archGO.transform, false);
                bannerGO.transform.localPosition = new Vector3(0, archHeight - 1f, 0);
                bannerGO.transform.localRotation = Quaternion.Euler(0, yaw, 0);
                bannerGO.transform.localScale = new Vector3(roadWidth + 1f, 1.5f, 1f);
                Object.DestroyImmediate(bannerGO.GetComponent<Collider>());
                bannerGO.GetComponent<MeshRenderer>().material = bannerMat;
            }

            // --- Particle burst (for runtime crossing effect) ---
            var particleGO = new GameObject("CrossingBurst");
            particleGO.transform.SetParent(parent, false);
            particleGO.transform.localPosition = new Vector3(0, 2f, 0);

            var ps = particleGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 1.5f;
            main.startSpeed = 8f;
            main.startSize = 0.15f;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(roadWidth, 0.5f, 0.5f);

            var pr = particleGO.GetComponent<ParticleSystemRenderer>();
            pr.material = new Material(Shader.Find("Particles/Standard Unlit"));
            pr.material.color = Color.white;

            ps.Stop();
        }

        static void BuildTerrain(SplineContainer container)
        {
            // Create a large ground mesh that follows the terrain elevation
            var terrainGO = new GameObject("Terrain");
            var mf = terrainGO.AddComponent<MeshFilter>();
            var mr = terrainGO.AddComponent<MeshRenderer>();

            var grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Grass.mat");
            if (grassMat == null)
            {
                grassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                grassMat.color = new Color(0.25f, 0.55f, 0.18f);
                AssetDatabase.CreateAsset(grassMat, "Assets/Materials/Grass.mat");
            }
            mr.material = grassMat;

            // Sample elevation along the spline to build a height map
            int gridSize = 80;
            float extent = 350f;
            var vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
            var uvs = new Vector2[(gridSize + 1) * (gridSize + 1)];
            var normals = new Vector3[(gridSize + 1) * (gridSize + 1)];

            float totalLength = container.CalculateLength();

            // Pre-sample spline at high resolution for accurate terrain height
            int splineSamples = 500;
            var splinePoints = new Vector3[splineSamples];
            for (int s = 0; s < splineSamples; s++)
            {
                float t = s / (float)splineSamples;
                container.Evaluate(t, out float3 sp, out _, out _);
                splinePoints[s] = (Vector3)sp;
            }

            for (int z = 0; z <= gridSize; z++)
            {
                for (int x = 0; x <= gridSize; x++)
                {
                    float wx = (x / (float)gridSize - 0.5f) * extent * 2f;
                    float wz = (z / (float)gridSize - 0.5f) * extent * 2f;

                    // Find closest pre-sampled spline point
                    float bestDist = float.MaxValue;
                    float bestY = 0f;

                    for (int s = 0; s < splineSamples; s++)
                    {
                        float dx = wx - splinePoints[s].x;
                        float dz = wz - splinePoints[s].z;
                        float d2 = dx * dx + dz * dz;
                        if (d2 < bestDist)
                        {
                            bestDist = d2;
                            bestY = splinePoints[s].y;
                        }
                    }

                    float distFromTrack = Mathf.Sqrt(bestDist);

                    // Close to road: sit just below road surface
                    // Far from road: blend down to base level
                    float roadEdge = 6f;    // road half-width + margin
                    float blendDist = 80f;  // distance over which terrain fades to flat

                    float y;
                    if (distFromTrack < roadEdge)
                    {
                        // Under the road — match road elevation closely
                        y = bestY - 0.3f;
                    }
                    else
                    {
                        // Blend from road elevation down to base
                        float blend = Mathf.Clamp01((distFromTrack - roadEdge) / blendDist);
                        y = Mathf.Lerp(bestY - 0.3f, -0.5f, blend);
                    }

                    int idx = z * (gridSize + 1) + x;
                    vertices[idx] = new Vector3(wx, y, wz);
                    uvs[idx] = new Vector2(x / (float)gridSize * 10f, z / (float)gridSize * 10f);
                    normals[idx] = Vector3.up;
                }
            }

            var triangles = new int[gridSize * gridSize * 6];
            int ti = 0;
            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    int bl = z * (gridSize + 1) + x;
                    int br = bl + 1;
                    int tl = bl + (gridSize + 1);
                    int tr = tl + 1;

                    triangles[ti++] = bl;
                    triangles[ti++] = tl;
                    triangles[ti++] = br;
                    triangles[ti++] = br;
                    triangles[ti++] = tl;
                    triangles[ti++] = tr;
                }
            }

            var mesh = new Mesh();
            mesh.name = "TerrainMesh";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mf.mesh = mesh;
        }

        static void BuildEnvironment(SplineContainer container)
        {
            var envGO = new GameObject("Environment");

            // Load URP tree prefabs
            string[] treeFolders = { "Ash", "Birch", "Chestnut", "Spruce", "Weeping Willow" };
            var treePrefabs = new System.Collections.Generic.List<GameObject>();

            foreach (var folder in treeFolders)
            {
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] { $"Assets/Realistic Tree/Prefabs/URP/{folder}" });
                foreach (var guid in guids)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                    if (prefab != null)
                        treePrefabs.Add(prefab);
                }
            }

            if (treePrefabs.Count == 0)
            {
                Debug.LogWarning("[BuildEnvironment] No tree prefabs found in Realistic Tree/Prefabs/URP/");
                return;
            }

            // Pre-sample spline for terrain height
            int splineSamples = 500;
            var splinePoints = new Vector3[splineSamples];
            for (int s = 0; s < splineSamples; s++)
            {
                float t = s / (float)splineSamples;
                container.Evaluate(t, out Unity.Mathematics.float3 sp, out _, out _);
                splinePoints[s] = (Vector3)sp;
            }

            var rng = new System.Random(42);
            int placed = 0;

            // Pass 1: Dense tree line along the road (8-40m from road, both sides)
            for (int s = 0; s < splineSamples; s += 2) // every ~3.5m along the track
            {
                Vector3 sp = splinePoints[s];
                // Get approximate road direction
                Vector3 next = splinePoints[(s + 1) % splineSamples];
                Vector3 fwd = (next - sp).normalized;
                Vector3 right = Vector3.Cross(fwd, Vector3.up).normalized;

                // Place trees on both sides
                foreach (float side in new[] { -1f, 1f })
                {
                    // 1-3 trees per side at this point
                    int count = rng.Next(1, 4);
                    for (int t = 0; t < count; t++)
                    {
                        float dist = 8f + (float)rng.NextDouble() * 32f; // 8-40m from center
                        float along = ((float)rng.NextDouble() - 0.5f) * 5f; // jitter along track

                        float x = sp.x + right.x * dist * side + fwd.x * along;
                        float z = sp.z + right.z * dist * side + fwd.z * along;

                        // Get terrain height at this position
                        float bestDist2 = float.MaxValue;
                        float bestY = 0f;
                        for (int ss = 0; ss < splineSamples; ss++)
                        {
                            float dx = x - splinePoints[ss].x;
                            float dz = z - splinePoints[ss].z;
                            float d2 = dx * dx + dz * dz;
                            if (d2 < bestDist2)
                            {
                                bestDist2 = d2;
                                bestY = splinePoints[ss].y;
                            }
                        }
                        float distFromRoad = Mathf.Sqrt(bestDist2);
                        if (distFromRoad < 7f) continue;

                        float roadEdge = 6f;
                        float blendDist = 80f;
                        float y;
                        if (distFromRoad < roadEdge)
                            y = bestY - 0.3f;
                        else
                        {
                            float blend = Mathf.Clamp01((distFromRoad - roadEdge) / blendDist);
                            y = Mathf.Lerp(bestY - 0.3f, -0.5f, blend);
                        }

                        var prefab = treePrefabs[rng.Next(treePrefabs.Count)];
                        var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        tree.name = $"Tree_{placed}";
                        tree.transform.SetParent(envGO.transform);
                        tree.transform.position = new Vector3(x, y, z);
                        tree.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);

                        // Trees near the road are slightly smaller, further out are bigger
                        float scaleBase = Mathf.Lerp(0.5f, 1.2f, Mathf.InverseLerp(8f, 40f, distFromRoad));
                        float scaleVar = scaleBase * (0.8f + (float)rng.NextDouble() * 0.4f);
                        tree.transform.localScale = Vector3.one * scaleVar;
                        tree.isStatic = true;
                        placed++;
                    }
                }
            }

            // Pass 2: Scatter some distant background trees
            for (int i = 0; i < 40; i++)
            {
                float angle = (float)rng.NextDouble() * 360f;
                float radius = 50f + (float)rng.NextDouble() * 200f;
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Cos(rad) * radius;
                float z = Mathf.Sin(rad) * radius;

                float bestDist2 = float.MaxValue;
                float bestY = 0f;
                for (int s = 0; s < splineSamples; s++)
                {
                    float dx = x - splinePoints[s].x;
                    float dz = z - splinePoints[s].z;
                    float d2 = dx * dx + dz * dz;
                    if (d2 < bestDist2)
                    {
                        bestDist2 = d2;
                        bestY = splinePoints[s].y;
                    }
                }
                float distFromRoad = Mathf.Sqrt(bestDist2);
                if (distFromRoad < 7f) continue;

                float blendAmt = Mathf.Clamp01((distFromRoad - 6f) / 80f);
                float y = Mathf.Lerp(bestY - 0.3f, -0.5f, blendAmt);

                var prefab = treePrefabs[rng.Next(treePrefabs.Count)];
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                tree.name = $"Tree_{placed}";
                tree.transform.SetParent(envGO.transform);
                tree.transform.position = new Vector3(x, y, z);
                tree.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
                float scaleVar = 0.8f + (float)rng.NextDouble() * 0.6f;
                tree.transform.localScale = Vector3.one * scaleVar;
                tree.isStatic = true;
                placed++;
            }

            Debug.Log($"[BuildEnvironment] Placed {placed} trees from {treePrefabs.Count} prefab variants.");
        }

        static void BuildUI(Cycling.RiderMotor motor, Cycling.GearSystem gearSystem, Race.PositionTracker posTracker)
        {
            // HUD Canvas
            var hudCanvasGO = new GameObject("HUD Canvas");
            var hudCanvas = hudCanvasGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var hudScaler = hudCanvasGO.AddComponent<CanvasScaler>();
            hudScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            hudScaler.referenceResolution = new Vector2(1920, 1080);
            hudCanvasGO.AddComponent<GraphicRaycaster>();

            // HUD Panel
            var hudPanel = new GameObject("HUD Panel", typeof(RectTransform));
            hudPanel.transform.SetParent(hudCanvasGO.transform, false);
            var hpRT = hudPanel.GetComponent<RectTransform>();
            hpRT.anchorMin = Vector2.zero;
            hpRT.anchorMax = Vector2.zero;
            hpRT.pivot = Vector2.zero;
            hpRT.sizeDelta = new Vector2(320, 320);
            hpRT.anchoredPosition = new Vector2(20, 20);
            var hpImg = hudPanel.AddComponent<Image>();
            hpImg.color = new Color(0, 0, 0, 0.5f);

            // HUD text elements
            string[] labels = { "Power Text", "Speed Text", "Cadence Text", "HR Text", "Gear Text", "Gradient Text", "Position Text", "Lap Text", "Draft Text" };
            string[] defaults = { "200W", "30.0 km/h", "90 rpm", "130 bpm", "11/22", "0.0%", "P--/--", "Lap 1/3", "" };
            float[] yPositions = { -15, -50, -85, -115, -145, -175, -205, -235, -265 };
            float[] sizes = { 28, 28, 22, 22, 22, 22, 22, 22, 22 };
            var textComponents = new TextMeshProUGUI[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                var go = new GameObject(labels[i], typeof(RectTransform));
                go.transform.SetParent(hudPanel.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(15, yPositions[i]);
                rt.sizeDelta = new Vector2(0, sizes[i] + 10);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = defaults[i];
                tmp.fontSize = sizes[i];
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.color = Color.white;
                textComponents[i] = tmp;
            }

            // Countdown text
            var countdownGO = new GameObject("Countdown Text", typeof(RectTransform));
            countdownGO.transform.SetParent(hudCanvasGO.transform, false);
            var cdRT = countdownGO.GetComponent<RectTransform>();
            cdRT.anchorMin = new Vector2(0.5f, 0.5f);
            cdRT.anchorMax = new Vector2(0.5f, 0.5f);
            cdRT.pivot = new Vector2(0.5f, 0.5f);
            cdRT.anchoredPosition = new Vector2(0, 50);
            cdRT.sizeDelta = new Vector2(400, 200);
            var cdTmp = countdownGO.AddComponent<TextMeshProUGUI>();
            cdTmp.text = "3";
            cdTmp.fontSize = 120;
            cdTmp.alignment = TextAlignmentOptions.Center;
            cdTmp.color = Color.white;

            // Lap progress bar
            var barBG = new GameObject("Lap Progress BG", typeof(RectTransform));
            barBG.transform.SetParent(hudCanvasGO.transform, false);
            var bgRT = barBG.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 1);
            bgRT.anchorMax = new Vector2(1, 1);
            bgRT.pivot = new Vector2(0.5f, 1);
            bgRT.anchoredPosition = Vector2.zero;
            bgRT.sizeDelta = new Vector2(0, 8);
            barBG.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

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
            fillImg.fillAmount = 0f;

            var progressBar = barBG.AddComponent<UI.LapProgressBar>();
            var pbSO = new SerializedObject(progressBar);
            pbSO.FindProperty("riderMotor").objectReferenceValue = motor;
            pbSO.FindProperty("fillImage").objectReferenceValue = fillImg;
            pbSO.ApplyModifiedProperties();

            // Wire RaceHUD
            var raceHUD = hudCanvasGO.AddComponent<UI.RaceHUD>();
            var hudSO = new SerializedObject(raceHUD);
            hudSO.FindProperty("riderMotor").objectReferenceValue = motor;
            hudSO.FindProperty("gearSystem").objectReferenceValue = gearSystem;
            hudSO.FindProperty("positionTracker").objectReferenceValue = posTracker;
            hudSO.FindProperty("powerText").objectReferenceValue = textComponents[0];
            hudSO.FindProperty("speedText").objectReferenceValue = textComponents[1];
            hudSO.FindProperty("cadenceText").objectReferenceValue = textComponents[2];
            hudSO.FindProperty("hrText").objectReferenceValue = textComponents[3];
            hudSO.FindProperty("gearText").objectReferenceValue = textComponents[4];
            hudSO.FindProperty("gradientText").objectReferenceValue = textComponents[5];
            hudSO.FindProperty("positionText").objectReferenceValue = textComponents[6];
            hudSO.FindProperty("lapText").objectReferenceValue = textComponents[7];
            hudSO.FindProperty("draftText").objectReferenceValue = textComponents[8];
            hudSO.FindProperty("countdownText").objectReferenceValue = cdTmp;
            hudSO.ApplyModifiedProperties();

            // Pause UI
            var pausePanel = new GameObject("Pause Panel", typeof(RectTransform));
            pausePanel.transform.SetParent(hudCanvasGO.transform, false);
            var ppRT = pausePanel.GetComponent<RectTransform>();
            ppRT.anchorMin = Vector2.zero;
            ppRT.anchorMax = Vector2.one;
            ppRT.offsetMin = Vector2.zero;
            ppRT.offsetMax = Vector2.zero;
            pausePanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            var pauseTextGO = new GameObject("Pause Text", typeof(RectTransform));
            pauseTextGO.transform.SetParent(pausePanel.transform, false);
            var ptRT = pauseTextGO.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0.5f, 0.5f);
            ptRT.anchorMax = new Vector2(0.5f, 0.5f);
            ptRT.sizeDelta = new Vector2(600, 120);
            var ptTmp = pauseTextGO.AddComponent<TextMeshProUGUI>();
            ptTmp.text = "PAUSED";
            ptTmp.fontSize = 80;
            ptTmp.alignment = TextAlignmentOptions.Center;
            ptTmp.color = Color.white;

            var pauseSubGO = new GameObject("Pause Sub", typeof(RectTransform));
            pauseSubGO.transform.SetParent(pausePanel.transform, false);
            var psRT = pauseSubGO.GetComponent<RectTransform>();
            psRT.anchorMin = new Vector2(0.5f, 0.5f);
            psRT.anchorMax = new Vector2(0.5f, 0.5f);
            psRT.anchoredPosition = new Vector2(0, -60);
            psRT.sizeDelta = new Vector2(400, 40);
            var psTmp = pauseSubGO.AddComponent<TextMeshProUGUI>();
            psTmp.text = "Press ESC to resume";
            psTmp.fontSize = 24;
            psTmp.alignment = TextAlignmentOptions.Center;
            psTmp.color = new Color(0.7f, 0.7f, 0.7f);

            var pauseUI = hudCanvasGO.AddComponent<UI.PauseUI>();
            var puSO = new SerializedObject(pauseUI);
            puSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            puSO.FindProperty("pauseText").objectReferenceValue = ptTmp;
            puSO.ApplyModifiedProperties();

            // Debug Canvas
            var debugCanvasGO = new GameObject("Debug Canvas");
            var debugCanvas = debugCanvasGO.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvas.sortingOrder = 10;
            var debugScaler = debugCanvasGO.AddComponent<CanvasScaler>();
            debugScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            debugScaler.referenceResolution = new Vector2(1920, 1080);
            debugCanvasGO.AddComponent<GraphicRaycaster>();

            var debugPanel = new GameObject("Debug Panel", typeof(RectTransform));
            debugPanel.transform.SetParent(debugCanvasGO.transform, false);
            var dpRT = debugPanel.GetComponent<RectTransform>();
            dpRT.anchorMin = new Vector2(1, 0.5f);
            dpRT.anchorMax = new Vector2(1, 0.5f);
            dpRT.pivot = new Vector2(1, 0.5f);
            dpRT.sizeDelta = new Vector2(350, 375);
            dpRT.anchoredPosition = new Vector2(-20, 0);
            debugPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Add slider labels and sliders
            var titleTmp = CreateTmpChild(debugPanel.transform, "Title", "DEBUG (`)", new Vector2(15, -10), 20, Color.yellow);

            float sy = -45f;
            var powerLabel = CreateTmpChild(debugPanel.transform, "Power Label", "Power: 200W", new Vector2(15, sy), 16, Color.white);
            var powerSlider = CreateSliderChild(debugPanel.transform, "Power Slider", new Vector2(0, sy - 30));
            sy -= 75f;
            var cadenceLabel = CreateTmpChild(debugPanel.transform, "Cadence Label", "Cadence: 90 rpm", new Vector2(15, sy), 16, Color.white);
            var cadenceSlider = CreateSliderChild(debugPanel.transform, "Cadence Slider", new Vector2(0, sy - 30));
            sy -= 75f;
            var hrLabel = CreateTmpChild(debugPanel.transform, "HR Label", "HR: 130 bpm", new Vector2(15, sy), 16, Color.white);
            var hrSlider = CreateSliderChild(debugPanel.transform, "HR Slider", new Vector2(0, sy - 30));
            sy -= 75f;
            var diffLabel = CreateTmpChild(debugPanel.transform, "Difficulty Label", "Difficulty: 50%", new Vector2(15, sy), 16, Color.white);
            var diffSlider = CreateSliderChild(debugPanel.transform, "Difficulty Slider", new Vector2(0, sy - 30));

            var debugComp = debugCanvasGO.AddComponent<UI.DebugPanel>();
            var dbSO = new SerializedObject(debugComp);
            dbSO.FindProperty("riderMotor").objectReferenceValue = motor;
            dbSO.FindProperty("panel").objectReferenceValue = debugPanel;
            dbSO.FindProperty("powerSlider").objectReferenceValue = powerSlider;
            dbSO.FindProperty("cadenceSlider").objectReferenceValue = cadenceSlider;
            dbSO.FindProperty("hrSlider").objectReferenceValue = hrSlider;
            dbSO.FindProperty("difficultySlider").objectReferenceValue = diffSlider;
            dbSO.FindProperty("powerLabel").objectReferenceValue = powerLabel;
            dbSO.FindProperty("cadenceLabel").objectReferenceValue = cadenceLabel;
            dbSO.FindProperty("hrLabel").objectReferenceValue = hrLabel;
            dbSO.FindProperty("difficultyLabel").objectReferenceValue = diffLabel;
            dbSO.ApplyModifiedProperties();

            // Results Canvas
            var resultsCanvasGO = new GameObject("Results Canvas");
            var resultsCanvas = resultsCanvasGO.AddComponent<Canvas>();
            resultsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            resultsCanvas.sortingOrder = 20;
            var rScaler = resultsCanvasGO.AddComponent<CanvasScaler>();
            rScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            rScaler.referenceResolution = new Vector2(1920, 1080);
            resultsCanvasGO.AddComponent<GraphicRaycaster>();

            var rPanel = new GameObject("Results Panel", typeof(RectTransform));
            rPanel.transform.SetParent(resultsCanvasGO.transform, false);
            var rpRT = rPanel.GetComponent<RectTransform>();
            rpRT.anchorMin = new Vector2(0.2f, 0.1f);
            rpRT.anchorMax = new Vector2(0.8f, 0.9f);
            rpRT.offsetMin = Vector2.zero;
            rpRT.offsetMax = Vector2.zero;
            rPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);

            var rTitle = new GameObject("Title", typeof(RectTransform));
            rTitle.transform.SetParent(rPanel.transform, false);
            var rtRT = rTitle.GetComponent<RectTransform>();
            rtRT.anchorMin = new Vector2(0, 1); rtRT.anchorMax = new Vector2(1, 1);
            rtRT.pivot = new Vector2(0.5f, 1);
            rtRT.anchoredPosition = new Vector2(0, -20);
            rtRT.sizeDelta = new Vector2(0, 60);
            var rtTmp = rTitle.AddComponent<TextMeshProUGUI>();
            rtTmp.text = "RACE FINISHED";
            rtTmp.fontSize = 48;
            rtTmp.alignment = TextAlignmentOptions.Center;
            rtTmp.color = Color.yellow;

            var rText = new GameObject("Results Text", typeof(RectTransform));
            rText.transform.SetParent(rPanel.transform, false);
            var rrRT = rText.GetComponent<RectTransform>();
            rrRT.anchorMin = new Vector2(0.05f, 0.15f);
            rrRT.anchorMax = new Vector2(0.95f, 0.85f);
            rrRT.offsetMin = Vector2.zero;
            rrRT.offsetMax = Vector2.zero;
            var rrTmp = rText.AddComponent<TextMeshProUGUI>();
            rrTmp.fontSize = 24;
            rrTmp.alignment = TextAlignmentOptions.TopLeft;
            rrTmp.color = Color.white;

            var rBtn = new GameObject("Menu Button", typeof(RectTransform));
            rBtn.transform.SetParent(rPanel.transform, false);
            var rbRT = rBtn.GetComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(0.3f, 0);
            rbRT.anchorMax = new Vector2(0.7f, 0);
            rbRT.pivot = new Vector2(0.5f, 0);
            rbRT.anchoredPosition = new Vector2(0, 20);
            rbRT.sizeDelta = new Vector2(0, 50);
            var rbImg = rBtn.AddComponent<Image>();
            rbImg.color = new Color(0.2f, 0.5f, 1f);
            var btn = rBtn.AddComponent<Button>();
            btn.targetGraphic = rbImg;
            var rbTextGO = new GameObject("Text", typeof(RectTransform));
            rbTextGO.transform.SetParent(rBtn.transform, false);
            var rbtRT = rbTextGO.GetComponent<RectTransform>();
            rbtRT.anchorMin = Vector2.zero; rbtRT.anchorMax = Vector2.one;
            rbtRT.offsetMin = Vector2.zero; rbtRT.offsetMax = Vector2.zero;
            var rbtTmp = rbTextGO.AddComponent<TextMeshProUGUI>();
            rbtTmp.text = "BACK TO MENU";
            rbtTmp.fontSize = 24;
            rbtTmp.alignment = TextAlignmentOptions.Center;
            rbtTmp.color = Color.white;

            var resultsUI = resultsCanvasGO.AddComponent<UI.RaceResultsUI>();
            var ruSO = new SerializedObject(resultsUI);
            ruSO.FindProperty("resultsPanel").objectReferenceValue = rPanel;
            ruSO.FindProperty("titleText").objectReferenceValue = rtTmp;
            ruSO.FindProperty("resultsText").objectReferenceValue = rrTmp;
            ruSO.FindProperty("menuButton").objectReferenceValue = btn;
            ruSO.ApplyModifiedProperties();
        }

        static TextMeshProUGUI CreateTmpChild(Transform parent, string name, string text, Vector2 pos, float fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(0, fontSize + 10);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = color;
            return tmp;
        }

        static Slider CreateSliderChild(Transform parent, string name, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 1);
            rt.anchorMax = new Vector2(0.95f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(0, 20);

            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.sizeDelta = Vector2.zero;
            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0.5f, 1); fillRt.sizeDelta = Vector2.zero;
            fill.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one; haRt.sizeDelta = new Vector2(-20, 0);
            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(20, 0); hRt.anchorMin = Vector2.zero; hRt.anchorMax = new Vector2(0, 1);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handleImg;
            return slider;
        }
    }
}
