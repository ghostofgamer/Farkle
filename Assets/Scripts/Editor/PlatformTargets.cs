using System;
using System.Collections.Generic;
using System.Linq;
using Farkle.Platform;
using UnityEditor;
using UnityEditor.Build;

namespace Farkle.Editor
{
    /// <summary>Соответствие площадки целевой платформе Unity, шаблону WebGL и папке плагинов.</summary>
    public static class PlatformTargets
    {
        public const string PlatformRoot = "Assets/Scripts/Platform";

        public static BuildTarget BuildTargetFor(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.Yandex:
                case PlatformId.VKPlay:
                case PlatformId.VKGames:
                    return BuildTarget.WebGL;
                case PlatformId.RuStore:
                    return BuildTarget.Android;
                default:
                    return EditorUserBuildSettings.activeBuildTarget;
            }
        }

        public static NamedBuildTarget NamedBuildTargetFor(BuildTarget target)
        {
            return NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
        }

        /// <summary>Имя WebGL-шаблона в формате PlayerSettings.WebGL.template.</summary>
        public static string WebGLTemplateFor(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.Yandex: return "PROJECT:Yandex";
                case PlatformId.VKPlay: return "PROJECT:VKPlay";
                case PlatformId.VKGames: return "PROJECT:VKGames";
                default: return "APPLICATION:Default";
            }
        }

        /// <summary>Папка с нативными плагинами площадки (.jslib, .aar). Null для Stub.</summary>
        public static string PluginsFolderFor(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.Yandex: return PlatformRoot + "/Yandex/Plugins";
                case PlatformId.VKPlay: return PlatformRoot + "/VKPlay/Plugins";
                case PlatformId.RuStore: return PlatformRoot + "/RuStore/Plugins";
                case PlatformId.VKGames: return PlatformRoot + "/VKGames/Plugins";
                default: return null;
            }
        }

        public static readonly PlatformId[] SdkPlatforms =
        {
            PlatformId.Yandex, PlatformId.VKPlay, PlatformId.RuStore, PlatformId.VKGames,
        };

        /// <summary>Определяет площадку по списку defines. Бросает исключение, если задано больше одного FARKLE_*.</summary>
        public static PlatformId ParseDefines(IEnumerable<string> defines)
        {
            var found = SdkPlatforms.Where(p => defines.Contains(PlatformDefines.For(p))).ToArray();
            if (found.Length > 1)
                throw new InvalidOperationException(
                    "More than one platform define is set: " + string.Join(", ", found.Select(PlatformDefines.For)) +
                    ". Use menu Farkle/Platform to pick exactly one.");
            return found.Length == 1 ? found[0] : PlatformId.Stub;
        }

        public static List<string> GetDefines(NamedBuildTarget target)
        {
            return PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .Where(d => d.Length > 0)
                .ToList();
        }
    }
}
