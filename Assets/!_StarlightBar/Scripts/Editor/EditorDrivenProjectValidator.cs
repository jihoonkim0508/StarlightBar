using System.Collections.Generic;
using StarlightBar.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarlightBar.Editor
{
    /// <summary>
    /// 씬을 재생성하거나 저장하지 않고 View의 직렬화 참조 누락만 검사합니다.
    /// </summary>
    public static class EditorDrivenProjectValidator
    {
        private const string SceneRoot = "Assets/!_StarlightBar/Scenes";

        /// <summary>
        /// 모든 정식 씬의 View 참조를 읽기 전용으로 검사합니다.
        /// </summary>
        [MenuItem("별빛주점/씬·프리팹 참조 검증")]
        public static void ValidateAll()
        {
            var original = SceneManager.GetActiveScene().path;
            var issues = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { SceneRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                ValidateScene(scene, issues);
            }
            if (!string.IsNullOrWhiteSpace(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);

            if (issues.Count == 0)
            {
                Debug.Log("별빛주점 View 검증 완료: 누락된 직렬화 참조가 없습니다.");
                return;
            }
            Debug.LogError("별빛주점 View 참조 누락:\n" + string.Join("\n", issues));
        }

        private static void ValidateScene(Scene scene, ICollection<string> issues)
        {
            foreach (var root in scene.GetRootGameObjects())
            foreach (var view in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (view == null || view.GetType().Namespace != "StarlightBar.UI")
                    continue;
                var serialized = new SerializedObject(view);
                var property = serialized.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.name == "m_Script" ||
                        property.propertyType != SerializedPropertyType.ObjectReference ||
                        property.objectReferenceValue != null)
                        continue;
                    issues.Add($"{scene.name}/{GetPath(view.transform)} · " +
                               $"{view.GetType().Name}.{property.displayName}");
                }
            }
        }

        private static string GetPath(Transform transform)
        {
            var path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}
