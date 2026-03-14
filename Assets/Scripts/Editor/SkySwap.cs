using UnityEditor;
using UnityEngine;

namespace Cycling.Editor
{
    public static class SkySwap
    {
        [MenuItem("Cycling/Set Morning Sky")]
        public static void SetMorningSky()
        {
            var hdr = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Sky/HdrSkyMorning004_HDR_8K.exr");
            if (hdr == null)
            {
                Debug.LogError("HdrSkyMorning004_HDR_8K.exr not found in Assets/Textures/Sky/");
                return;
            }

            var importer = AssetImporter.GetAtPath("Assets/Textures/Sky/HdrSkyMorning004_HDR_8K.exr") as TextureImporter;
            if (importer != null)
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
                hdr = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Sky/HdrSkyMorning004_HDR_8K.exr");
            }

            var skyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Skybox.mat");
            if (skyMat == null)
            {
                skyMat = new Material(Shader.Find("Skybox/Panoramic"));
                AssetDatabase.CreateAsset(skyMat, "Assets/Materials/Skybox.mat");
            }

            skyMat.SetTexture("_MainTex", hdr);
            skyMat.SetFloat("_Exposure", 1.1f);
            EditorUtility.SetDirty(skyMat);

            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            AssetDatabase.SaveAssets();

            Debug.Log("Skybox set to HdrSkyMorning004.");
        }
    }
}
