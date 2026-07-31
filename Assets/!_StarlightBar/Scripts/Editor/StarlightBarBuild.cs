using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StarlightBar.Editor
{
    /// <summary>
    /// 검증된 빌드 설정 씬을 사용해 Windows 정식판을 재현 가능하게 생성합니다.
    /// </summary>
    public static class StarlightBarBuild
    {
        /// <summary>
        /// 명령줄과 에디터 메뉴에서 동일한 Windows x64 빌드를 생성합니다.
        /// </summary>
        [MenuItem("별빛주점/Windows 정식 빌드")]
        public static void BuildWindowsPlayer()
        {
            var outputRoot = Environment.GetEnvironmentVariable("STARLIGHT_BUILD_DIR");
            if (string.IsNullOrWhiteSpace(outputRoot))
                outputRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Windows"));
            Directory.CreateDirectory(outputRoot);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0 || !scenes[0].EndsWith("/Bootstrap.unity", StringComparison.Ordinal))
                throw new InvalidOperationException("Bootstrap이 첫 번째인 빌드 씬 목록이 필요합니다.");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(outputRoot, "StarlightBar.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CleanBuildCache
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException(
                    $"Windows 빌드 실패: {report.summary.result} / 오류 {report.summary.totalErrors}개");

            Debug.Log(
                $"별빛주점 Windows 빌드 완료: {options.locationPathName} " +
                $"({report.summary.totalSize / (1024f * 1024f):0.0} MB)");
        }
    }
}
