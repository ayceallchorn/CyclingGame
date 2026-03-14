using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cycling.Editor
{
    public static class TextureSetup
    {
        [MenuItem("Cycling/Apply Textures")]
        public static void ApplyTextures()
        {
            SetupRoadMaterial();
            SetupGrassMaterial();
            SetupSkybox();
            AssetDatabase.SaveAssets();
            Debug.Log("Textures applied: road, grass, skybox.");
        }

        static void SetupRoadMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Road.mat");
            if (mat == null) return;

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Road/CityStreetAsphaltGenericClean001_COL_2K.jpg");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Road/CityStreetAsphaltGenericClean001_NRM_2K.jpg");
            var gloss = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Road/CityStreetAsphaltGenericClean001_GLOSS_2K.jpg");
            var ao = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Road/CityStreetAsphaltGenericClean001_AO_2K.jpg");

            // Set normal map import settings
            if (normal != null)
            {
                var importer = AssetImporter.GetAtPath("Assets/Textures/Road/CityStreetAsphaltGenericClean001_NRM_2K.jpg") as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                    normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Road/CityStreetAsphaltGenericClean001_NRM_2K.jpg");
                }
            }

            if (albedo != null) mat.SetTexture("_BaseMap", albedo);
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (ao != null)
            {
                mat.SetTexture("_OcclusionMap", ao);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }
            // URP Lit uses smoothness (inverse of roughness) — gloss map IS smoothness
            if (gloss != null)
            {
                mat.SetFloat("_Smoothness", 0.3f);
            }

            mat.SetColor("_BaseColor", Color.white);
            // Tile the road texture along the road
            mat.SetTextureScale("_BaseMap", new Vector2(1f, 8f));
            mat.SetTextureScale("_BumpMap", new Vector2(1f, 8f));
            mat.SetTextureScale("_OcclusionMap", new Vector2(1f, 8f));

            EditorUtility.SetDirty(mat);
        }

        static void SetupGrassMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Grass.mat");
            if (mat == null) return;

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Grass/Poliigon_GrassPatchyGround_4585_BaseColor.jpg");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Grass/Poliigon_GrassPatchyGround_4585_Normal.png");
            var roughness = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Grass/Poliigon_GrassPatchyGround_4585_Roughness.jpg");
            var ao = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Grass/Poliigon_GrassPatchyGround_4585_AmbientOcclusion.jpg");

            // Set normal map import settings
            if (normal != null)
            {
                var importer = AssetImporter.GetAtPath("Assets/Textures/Grass/Poliigon_GrassPatchyGround_4585_Normal.png") as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                    normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Grass/Poliigon_GrassPatchyGround_4585_Normal.png");
                }
            }

            if (albedo != null) mat.SetTexture("_BaseMap", albedo);
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (ao != null)
            {
                mat.SetTexture("_OcclusionMap", ao);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }

            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.15f);
            // Tile grass across terrain
            mat.SetTextureScale("_BaseMap", new Vector2(20f, 20f));
            mat.SetTextureScale("_BumpMap", new Vector2(20f, 20f));
            mat.SetTextureScale("_OcclusionMap", new Vector2(20f, 20f));

            EditorUtility.SetDirty(mat);
        }

        static void SetupSkybox()
        {
            var hdr = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Sky/HdrOutdoorFieldDayOvercast004_HDR_8K.exr");
            if (hdr == null)
            {
                Debug.LogWarning("HDR sky texture not found.");
                return;
            }

            // Set HDR texture import settings
            var importer = AssetImporter.GetAtPath("Assets/Textures/Sky/HdrOutdoorFieldDayOvercast004_HDR_8K.exr") as TextureImporter;
            if (importer != null)
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
                hdr = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Sky/HdrOutdoorFieldDayOvercast004_HDR_8K.exr");
            }

            // Create panoramic skybox material
            var skyMat = new Material(Shader.Find("Skybox/Panoramic"));
            skyMat.SetTexture("_MainTex", hdr);
            skyMat.SetFloat("_Exposure", 1.0f);

            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.CreateAsset(skyMat, "Assets/Materials/Skybox.mat");

            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();

            EditorUtility.SetDirty(skyMat);
        }
    }
}
