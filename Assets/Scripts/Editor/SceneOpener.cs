using UnityEditor;
using UnityEditor.SceneManagement;

namespace Cycling.Editor
{
    public static class SceneOpener
    {
        [MenuItem("Cycling/Open RaceScene")]
        public static void OpenRaceScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/RaceScene.unity");
        }
    }
}
