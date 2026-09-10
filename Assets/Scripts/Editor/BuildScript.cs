using System.Linq;
using Farkle.Platform;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Farkle.Editor
{
    /// <summary>
    /// Сборка под площадку одной командой: переключает площадку и запускает BuildPipeline.
    /// Меню Farkle/Build или из командной строки:
    ///   Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod Farkle.Editor.BuildScript.BuildYandex
    ///   Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod Farkle.Editor.BuildScript.BuildVKPlay
    ///   Unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod Farkle.Editor.BuildScript.BuildRuStore
    /// Результат кладётся в Builds/&lt;площадка&gt;.
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("Farkle/Build/Yandex Games (WebGL)", priority = 0)]
        public static void BuildYandex() => Build(PlatformId.Yandex);

        [MenuItem("Farkle/Build/VK Play (WebGL)", priority = 1)]
        public static void BuildVKPlay() => Build(PlatformId.VKPlay);

        [MenuItem("Farkle/Build/RuStore (Android APK)", priority = 2)]
        public static void BuildRuStore() => Build(PlatformId.RuStore);

        public static void Build(PlatformId platform)
        {
            PlatformSwitcher.Apply(platform);

            var target = PlatformTargets.BuildTargetFor(platform);
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Fail("No scenes enabled in Build Settings");
                return;
            }

            var location = target == BuildTarget.Android
                ? $"Builds/{platform}/Farkle.apk"
                : $"Builds/{platform}";

            if (target == BuildTarget.Android)
                EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                locationPathName = location,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Farkle] Build {platform} succeeded: {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime:mm\\:ss})");
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            else
            {
                Fail($"Build {platform} finished with {summary.result}: {summary.totalErrors} errors");
            }
        }

        private static void Fail(string message)
        {
            Debug.LogError("[Farkle] " + message);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
