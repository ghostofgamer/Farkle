using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.RuStore
{
    /// <summary>
    /// Реклама для Android-сборки. TODO: выбрать сеть (Yandex Mobile Ads Unity plugin или myTarget)
    /// и подключить InterstitialAdLoader / RewardedAdLoader.
    /// </summary>
    public sealed class RuStoreAdsService : IAdsService
    {
        public bool IsInterstitialAvailable => false;
        public bool IsRewardedAvailable => false;

        public event Action AdOpened;
        public event Action AdClosed;

        public UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[RuStore] ShowInterstitial: ad network not integrated yet");
            return UniTask.FromResult(false);
        }

        public UniTask<RewardedAdResult> ShowRewardedAsync(string placement, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning($"[RuStore] ShowRewarded '{placement}': ad network not integrated yet");
            return UniTask.FromResult(RewardedAdResult.NotAvailable);
        }

        private void RaiseOpened() => AdOpened?.Invoke();
        private void RaiseClosed() => AdClosed?.Invoke();
    }
}
