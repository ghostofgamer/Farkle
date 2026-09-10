using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.Yandex
{
    /// <summary>
    /// Яндекс Игры. TODO: подключить Yandex Games SDK (ysdk) через .jslib в папке Plugins.
    /// Соответствие: InitializeAsync -> YaGames.init, NotifyGameReady -> ysdk.features.LoadingAPI.ready(),
    /// NotifyGameplayStart/Stop -> ysdk.features.GameplayAPI.start()/stop(), AuthorizeAsync -> ysdk.auth.openAuthDialog().
    /// </summary>
    public sealed class YandexPlatformService : IPlatformService
    {
        public PlatformId Platform => PlatformId.Yandex;
        public bool IsInitialized { get; private set; }
        public string Language { get; private set; } = "ru";
        public bool IsAuthorized { get; private set; }
        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }

        public UniTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] SDK not integrated yet, running as no-op");
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public void NotifyGameReady() => Debug.Log("[Yandex] GameReady (TODO: LoadingAPI.ready)");
        public void NotifyGameplayStart() => Debug.Log("[Yandex] GameplayStart (TODO: GameplayAPI.start)");
        public void NotifyGameplayStop() => Debug.Log("[Yandex] GameplayStop (TODO: GameplayAPI.stop)");

        public UniTask<bool> AuthorizeAsync(CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] AuthorizeAsync: SDK not integrated yet");
            return UniTask.FromResult(false);
        }
    }
}
