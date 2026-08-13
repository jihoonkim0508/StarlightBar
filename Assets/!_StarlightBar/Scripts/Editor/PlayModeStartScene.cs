#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace StarlightBar.Editor
{
    [InitializeOnLoad]
    internal static class PlayModeStartScene
    {
        private const string BootstrapPath =
            "Assets/!_StarlightBar/Scenes/Bootstrap.unity";

        static PlayModeStartScene()
        {
            var bootstrap = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapPath);
            if (bootstrap != null && EditorSceneManager.playModeStartScene != bootstrap)
                EditorSceneManager.playModeStartScene = bootstrap;
        }
    }
}
#endif
