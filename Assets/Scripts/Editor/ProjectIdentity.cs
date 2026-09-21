using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Farkle.Editor
{
    /// <summary>
    /// Имя компании, название и идентификатор приложения в одном месте.
    /// Выставляются в PlayerSettings при переключении площадки и перед каждой сборкой,
    /// поэтому для новой игры на основе проекта достаточно поменять константы здесь.
    ///
    /// Идентификатор Android-пакета после первой публикации в магазине менять нельзя:
    /// для магазина это уже другое приложение.
    /// </summary>
    public static class ProjectIdentity
    {
        public const string CompanyName = "ghostofgamer";

        /// <summary>Название игры: подпись под иконкой на Android и заголовок вкладки в браузере.</summary>
        public const string ProductName = "Farkle";

        public const string AndroidPackage = "ru.ghostofgamer.farkle";

        public static void Apply()
        {
            if (PlayerSettings.companyName != CompanyName)
            {
                Debug.Log($"[Farkle] Company name: {PlayerSettings.companyName} -> {CompanyName}");
                PlayerSettings.companyName = CompanyName;
            }

            if (PlayerSettings.productName != ProductName)
            {
                Debug.Log($"[Farkle] Product name: {PlayerSettings.productName} -> {ProductName}");
                PlayerSettings.productName = ProductName;
            }

            var current = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            if (current != AndroidPackage)
            {
                Debug.Log($"[Farkle] Android package: {current} -> {AndroidPackage}");
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidPackage);
            }
        }
    }
}
