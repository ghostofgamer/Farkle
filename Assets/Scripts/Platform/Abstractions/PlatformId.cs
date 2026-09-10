namespace Farkle.Platform
{
    /// <summary>Целевая площадка, под которую собран билд.</summary>
    public enum PlatformId
    {
        /// <summary>Заглушка: редактор и локальные тесты без SDK.</summary>
        Stub = 0,
        /// <summary>Яндекс Игры, WebGL.</summary>
        Yandex = 1,
        /// <summary>VK Play, WebGL.</summary>
        VKPlay = 2,
        /// <summary>RuStore, нативный Android.</summary>
        RuStore = 3,
    }
}
