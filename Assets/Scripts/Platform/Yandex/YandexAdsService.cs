using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.Yandex
{
    /// <summary>
    /// Реклама Яндекс Игр. TODO: ysdk.adv.showFullscreenAdv и ysdk.adv.showRewardedVideo.
    /// Между показами fullscreen-рекламы у Яндекса есть минимальный интервал, SDK сам его контролирует.
    /// </summary>
    public sealed class YandexAdsService : IAdsService
    {
        public bool IsInterstitialAvailable => false;
        public bool IsRewardedAvailable => false;

        public event Action AdOpened;
        public event Action AdClosed;

        public UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] ShowInterstitial: SDK not integrated yet");
            return UniTask.FromResult(false);
        }

        public UniTask<RewardedAdResult> ShowRewardedAsync(string placement, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning($"[Yandex] ShowRewarded '{placement}': SDK not integrated yet");
            return UniTask.FromResult(RewardedAdResult.NotAvailable);
        }

        // Заглушки, чтобы компилятор не ругался на неиспользуемые события до интеграции SDK.
        private void RaiseOpened() => AdOpened?.Invoke();
        private void RaiseClosed() => AdClosed?.Invoke();
    }
}
