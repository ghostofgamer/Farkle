using Farkle.Platform.Stub;
using Zenject;

namespace Farkle.Platform.Installers
{
    /// <summary>
    /// Единственное место, где выбирается реализация площадки.
    /// В редакторе всегда Stub: SDK площадок работают только в реальном билде.
    /// В билде реализация выбирается по define FARKLE_YANDEX / FARKLE_VKPLAY / FARKLE_RUSTORE,
    /// без define используется Stub.
    /// </summary>
    public sealed class PlatformInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
#if UNITY_EDITOR || !(FARKLE_YANDEX || FARKLE_VKPLAY || FARKLE_RUSTORE)
            Container.Bind<IPlatformService>().To<StubPlatformService>().AsSingle();
            Container.Bind<IAdsService>().To<StubAdsService>().AsSingle();
            Container.Bind<IPurchaseService>().To<StubPurchaseService>().AsSingle();
            Container.Bind<ICloudSaveService>().To<StubCloudSaveService>().AsSingle();
            Container.Bind<ILeaderboardService>().To<StubLeaderboardService>().AsSingle();
#elif FARKLE_YANDEX
            Container.Bind<IPlatformService>().To<Yandex.YandexPlatformService>().AsSingle();
            Container.Bind<IAdsService>().To<Yandex.YandexAdsService>().AsSingle();
            Container.Bind<IPurchaseService>().To<Yandex.YandexPurchaseService>().AsSingle();
            Container.Bind<ICloudSaveService>().To<Yandex.YandexCloudSaveService>().AsSingle();
            Container.Bind<ILeaderboardService>().To<Yandex.YandexLeaderboardService>().AsSingle();
#elif FARKLE_VKPLAY
            Container.Bind<IPlatformService>().To<VKPlay.VKPlayPlatformService>().AsSingle();
            Container.Bind<IAdsService>().To<VKPlay.VKPlayAdsService>().AsSingle();
            Container.Bind<IPurchaseService>().To<VKPlay.VKPlayPurchaseService>().AsSingle();
            Container.Bind<ICloudSaveService>().To<VKPlay.VKPlayCloudSaveService>().AsSingle();
            Container.Bind<ILeaderboardService>().To<VKPlay.VKPlayLeaderboardService>().AsSingle();
#elif FARKLE_RUSTORE
            Container.Bind<IPlatformService>().To<RuStore.RuStorePlatformService>().AsSingle();
            Container.Bind<IAdsService>().To<RuStore.RuStoreAdsService>().AsSingle();
            Container.Bind<IPurchaseService>().To<RuStore.RuStorePurchaseService>().AsSingle();
            Container.Bind<ICloudSaveService>().To<RuStore.RuStoreCloudSaveService>().AsSingle();
            Container.Bind<ILeaderboardService>().To<RuStore.RuStoreLeaderboardService>().AsSingle();
#endif

            Container.BindInterfacesTo<PlatformInitializer>().AsSingle();
        }
    }
}
