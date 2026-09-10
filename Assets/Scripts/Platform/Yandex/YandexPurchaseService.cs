using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.Yandex
{
    /// <summary>
    /// Покупки Яндекс Игр. TODO: ysdk.getPayments({ signed: true }), payments.getCatalog,
    /// payments.purchase, payments.getPurchases, payments.consumePurchase.
    /// </summary>
    public sealed class YandexPurchaseService : IPurchaseService
    {
        private static readonly IReadOnlyList<ProductInfo> Empty = new ProductInfo[0];
        private static readonly IReadOnlyList<PurchaseInfo> NoPurchases = new PurchaseInfo[0];

        public bool IsAvailable => false;

        public UniTask<IReadOnlyList<ProductInfo>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] GetProducts: SDK not integrated yet");
            return UniTask.FromResult(Empty);
        }

        public UniTask<PurchaseResult> PurchaseAsync(string productId, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning($"[Yandex] Purchase '{productId}': SDK not integrated yet");
            return UniTask.FromResult(PurchaseResult.Fail("not_integrated"));
        }

        public UniTask<IReadOnlyList<PurchaseInfo>> GetPendingPurchasesAsync(CancellationToken cancellationToken = default)
        {
            return UniTask.FromResult(NoPurchases);
        }

        public UniTask ConsumeAsync(string purchaseToken, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] Consume: SDK not integrated yet");
            return UniTask.CompletedTask;
        }
    }
}
