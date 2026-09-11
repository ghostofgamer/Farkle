using System;
using Cysharp.Threading.Tasks;
using Farkle.Core.Localization;
using Farkle.Game.Quality;
using UnityEngine;
using Zenject;

namespace Farkle.Platform.Installers
{
    /// <summary>
    /// Запускает инициализацию SDK площадки сразу после сборки контейнера,
    /// выставляет язык интерфейса и качество графики по данным площадки
    /// и сообщает площадке, что игра загрузилась.
    ///
    /// Сигнал готовности обязателен для модерации Яндекс Игр: до него платформа
    /// держит свой лоадер в состоянии ожидания и считает SDK невстроенным.
    /// Когда появится настоящий экран загрузки, вызов NotifyGameReady нужно перенести
    /// в момент показа первого игрового экрана и убрать отсюда.
    /// </summary>
    public sealed class PlatformInitializer : IInitializable
    {
        private readonly IPlatformService _platform;
        private readonly ILocalization _localization;
        private readonly IQualityService _quality;

        public PlatformInitializer(IPlatformService platform, ILocalization localization, IQualityService quality)
        {
            _platform = platform;
            _localization = localization;
            _quality = quality;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            Debug.Log($"[Platform] Initializing {_platform.Platform}");

            try
            {
                await _platform.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Platform] Initialization failed: {e.Message}");
            }

            // Язык интерфейса определяется автоматически по языку площадки.
            // Требование модерации Яндекс Игр, п. 2.14.
            _localization.SetLanguage(_platform.Language);
            Debug.Log($"[Platform] Language from SDK: '{_platform.Language}' -> '{_localization.Language}'");

            // Тип устройства площадки известен только после инициализации SDK.
            // Сбой качества не должен помешать сигналу готовности ниже.
            try
            {
                _quality.ApplyForDevice(_platform.Device);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Platform] Quality setup failed: {e.Message}");
            }

            // Сообщаем о готовности даже после неудачной инициализации:
            // если SDK жив, но упал один из вызовов, лоадер платформы всё равно нужно закрыть.
            _platform.NotifyGameReady();
            Debug.Log($"[Platform] {_platform.Platform} ready, initialized={_platform.IsInitialized}");
        }
    }
}
