using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace StarlightBar.Editor
{
    /// <summary>
    /// 원본 Noto Sans KR에서 씬 전환에도 파괴되지 않는 영구 TMP 동적 폰트 자산을 생성합니다.
    /// </summary>
    public static class KoreanFontAssetBuilder
    {
        private const string SourceFontPath =
            "Assets/!_StarlightBar/Resources/StarlightBar/Fonts/NotoSansKR-Variable.ttf";
        private const string FontAssetPath =
            "Assets/!_StarlightBar/Resources/StarlightBar/Fonts/NotoSansKR-Dynamic.asset";
        private const string ProjectContentRoot = "Assets/!_StarlightBar";

        /// <summary>
        /// 한글 TMP 폰트와 기본 재료·아틀라스를 영구 서브에셋으로 생성하거나 기존 자산을 검증합니다.
        /// </summary>
        [MenuItem("별빛주점/한글 TMP 폰트 자산 확인")]
        public static void EnsureAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null)
            {
                if (existing.material == null)
                    throw new InvalidOperationException("기존 한글 TMP 폰트의 재료가 누락되었습니다.");
                PrewarmProjectCharacters(existing);
                Debug.Log($"한글 TMP 폰트 자산이 준비되어 있습니다: {FontAssetPath}");
                return;
            }

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont == null)
                throw new InvalidOperationException($"원본 한글 폰트를 찾지 못했습니다: {SourceFontPath}");

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont, 64, 9, GlyphRenderMode.SDFAA, 4096, 4096,
                AtlasPopulationMode.Dynamic, false);
            if (fontAsset == null || fontAsset.material == null)
                throw new InvalidOperationException("Noto Sans KR TMP 폰트 생성에 실패했습니다.");

            fontAsset.material.name = "Noto Sans KR Dynamic Material";
            var material = fontAsset.material;
            var atlasTextures = fontAsset.atlasTextures;

            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
            fontAsset.name = "Noto Sans KR Dynamic";
            AssetDatabase.AddObjectToAsset(material, fontAsset);
            if (atlasTextures != null)
            {
                for (var index = 0; index < atlasTextures.Length; index++)
                {
                    var texture = atlasTextures[index];
                    if (texture == null)
                        continue;
                    texture.name = $"Noto Sans KR Dynamic Atlas {index}";
                    AssetDatabase.AddObjectToAsset(texture, fontAsset);
                }
            }

            PrewarmProjectCharacters(fontAsset);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(FontAssetPath, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"한글 TMP 폰트 자산을 생성했습니다: {FontAssetPath}");
        }

        private static void PrewarmProjectCharacters(TMP_FontAsset fontAsset)
        {
            var characters = CollectProjectCharacters();
            // 동적 모드에서 현재 프로젝트 문자를 추가한 뒤 정적으로 잠가야
            // 글리프·문자 테이블과 아틀라스가 에디터 재시작 후에도 자산에 남습니다.
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            if (!fontAsset.TryAddCharacters(characters, out var missingCharacters))
            {
                Debug.LogWarning(
                    $"한글 TMP 폰트에 추가하지 못한 문자가 있습니다: {missingCharacters}");
            }
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            // 편집 모드에서도 TMP가 즉시 글리프와 아틀라스를 읽도록 폰트와 모든 서브에셋을 저장합니다.
            EditorUtility.SetDirty(fontAsset);
            if (fontAsset.material != null)
                EditorUtility.SetDirty(fontAsset.material);
            foreach (var texture in fontAsset.atlasTextures ?? Array.Empty<Texture2D>())
            {
                if (texture != null)
                    EditorUtility.SetDirty(texture);
            }
            AssetDatabase.SaveAssets();
        }

        private static string CollectProjectCharacters()
        {
            var characters = new StringBuilder();
            for (var code = 32; code <= 126; code++)
                characters.Append((char)code);
            characters.Append("　…·—「」『』“”‘’✓□✦×→←↑↓");

            var extensions = new[]
            {
                ".cs", ".asset", ".unity", ".prefab", ".md", ".json", ".inputactions"
            };
            foreach (var file in Directory.EnumerateFiles(
                         ProjectContentRoot, "*.*", SearchOption.AllDirectories)
                     .Where(path => extensions.Contains(
                         Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)))
            {
                var content = File.ReadAllText(file, Encoding.UTF8);
                foreach (var character in content)
                {
                    if (IsKorean(character))
                        characters.Append(character);
                }
            }

            return new string(characters.ToString().Distinct().ToArray());
        }

        private static bool IsKorean(char character)
        {
            return character is >= '\uAC00' and <= '\uD7A3'
                or >= '\u1100' and <= '\u11FF'
                or >= '\u3130' and <= '\u318F';
        }

        /// <summary>
        /// Unity 배치 모드에서 한글 TMP 폰트 자산 생성을 실행합니다.
        /// </summary>
        public static void EnsureAssetBatch()
        {
            try
            {
                EnsureAsset();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
