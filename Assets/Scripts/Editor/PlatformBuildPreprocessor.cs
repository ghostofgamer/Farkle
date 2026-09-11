using System.Collections.Generic;
using Farkle.Platform;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Farkle.Editor
{
    /// <summary>
    /// Проверка перед любой сборкой (меню Build, Build Profiles, BuildScript, CLI):
    /// ровно один FARKLE_* define, целевая платформа соответствует площадке,
    /// WebGL-шаблон и сжатие выставлены, нативные плагины чужих площадок выключены.
    /// При несоответствии сборка останавливается с понятным сообщением.
    /// </summary>
    public sealed class PlatformBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            var target = report.summary.platform;
            var defines = CollectDefines(report);

            PlatformId platform;
            try
            {
                platform = PlatformTargets.ParseDefines(defines);
            }
            catch (System.InvalidOperationException e)
            {
                throw new BuildFailedException(e.Message);
            }

            if (platform != PlatformId.Stub)
            {
                var expected = PlatformTargets.BuildTargetFor(platform);
                if (expected != target)
                    throw new BuildFailedException(
                        $"[Farkle] Platform {platform} requires build target {expected}, but building for {target}. " +
                        "Use menu Farkle/Platform to switch.");
            }

            if (target == BuildTarget.WebGL)
            {
                var template = PlatformTargets.WebGLTemplateFor(platform);
                if (PlayerSettings.WebGL.template != template)
                {
                    Debug.Log($"[Farkle] Setting WebGL template to {template}");
                    PlayerSettings.WebGL.template = template;
                }

                // Сжатие зависит от того, как хостинг площадки отдаёт файлы, см. PlatformTargets.
                var compression = PlatformTargets.WebGLCompressionFor(platform);
                var fallback = PlatformTargets.WebGLDecompressionFallbackFor(platform);
                if (PlayerSettings.WebGL.compressionFormat != compression || PlayerSettings.WebGL.decompressionFallback != fallback)
                {
                    Debug.Log($"[Farkle] Setting WebGL compression to {compression}, decompression fallback {fallback}");
                    PlayerSettings.WebGL.compressionFormat = compression;
                    PlayerSettings.WebGL.decompressionFallback = fallback;
                }
            }

            var mismatches = PlatformPluginToggler.FindMismatches(platform);
            if (mismatches.Count > 0)
                throw new BuildFailedException(
                    $"[Farkle] Native plugins are not configured for platform {platform}:\n  " +
                    string.Join("\n  ", mismatches) +
                    "\nRun menu Farkle/Platform/<platform> before building.");

            Debug.Log($"[Farkle] Building platform {platform} for {target}");
        }

        private static List<string> CollectDefines(BuildReport report)
        {
            var named = NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup);
            var defines = PlatformTargets.GetDefines(named);

            // Build Profiles могут добавлять свои defines поверх PlayerSettings.
            var profile = BuildProfile.GetActiveBuildProfile();
            if (profile != null && profile.scriptingDefines != null)
                defines.AddRange(profile.scriptingDefines);

            return defines;
        }
    }
}
