using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Farkle.Platform.Installers
{
    /// <summary>
    /// Запускает инициализацию SDK площадки сразу после сборки контейнера.
    /// Игра ждёт IPlatformService.IsInitialized перед первым обращением к рекламе и покупкам.
    /// </summary>
    public sealed class PlatformInitializer : IInitializable
    {
        private readonly IPlatformService _platform;

        public PlatformInitializer(IPlatformService platform)
        {
            _platform = platform;
        }

        public void Initialize()
        {
            Debug.Log($"[Platform] Initializing {_platform.Platform}");
            _platform.InitializeAsync().Forget();
        }
    }
}
