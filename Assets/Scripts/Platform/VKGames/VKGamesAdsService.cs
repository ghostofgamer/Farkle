using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.VKGames
{
    /// <summary>
    /// Нативная реклама VK: VKWebAppShowNativeAds с форматами interstitial и reward.
    /// Перед показом мост проверяет наличие рекламы через VKWebAppCheckNativeAds.
    ///
    /// Правила VK: rewarded только по действию игрока и с понятной наградой,
    /// interstitial только на переходах между экранами и никогда сразу после запуска игры.
    /// До прохождения модерации реклама работает в тестовом режиме.
    /// </summary>
    public sealed class VKGamesAdsService : IAdsService
    {
        public VKGamesAdsService()
        {
            VKGamesBridge.AdOpened += () => AdOpened?.Invoke();
            VKGamesBridge.AdClosed += () => AdClosed?.Invoke();
        }

        /// <summary>Наличие рекламы проверяется непосредственно перед показом, поэтому здесь только готовность моста.</summary>
        public bool IsInterstitialAvailable => VKGamesBridge.IsSupported && VKGamesSession.IsInitialized;

        public bool IsRewardedAvailable => VKGamesBridge.IsSupported && VKGamesSession.IsInitialized;

        public event Action AdOpened;
        public event Action AdClosed;

        public async UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInterstitialAvailable)
                return false;

            try
            {
                return await VKGamesBridge.ShowInterstitialAsync(cancellationToken);
            }
            catch (VKGamesBridgeException e)
            {
                Debug.LogWarning($"[VKGames] Interstitial failed: {e.Message}");
                return false;
            }
        }

        public async UniTask<RewardedAdResult> ShowRewardedAsync(string placement, CancellationToken cancellationToken = default)
        {
            if (!IsRewardedAvailable)
                return RewardedAdResult.NotAvailable;

            try
            {
                // VK не сообщает отдельно, досмотрел ли игрок рекламу: result: true единственный признак успеха.
                // Что приходит при закрытии раньше времени, документация не описывает: проверить на живой площадке.
                var shown = await VKGamesBridge.ShowRewardedAsync(cancellationToken);
                return shown ? RewardedAdResult.Rewarded : RewardedAdResult.NotAvailable;
            }
            catch (VKGamesBridgeException e)
            {
                Debug.LogWarning($"[VKGames] Rewarded '{placement}' failed: {e.Message}");
                return RewardedAdResult.Failed;
            }
        }
    }
}
