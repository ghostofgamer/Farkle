namespace Farkle.Platform
{
    /// <summary>
    /// Scripting defines, по которым выбирается площадка.
    /// В билде должен быть ровно один из них. Без define используется Stub.
    /// </summary>
    public static class PlatformDefines
    {
        public const string Yandex = "FARKLE_YANDEX";
        public const string VKPlay = "FARKLE_VKPLAY";
        public const string RuStore = "FARKLE_RUSTORE";

        public static readonly string[] All = { Yandex, VKPlay, RuStore };

        public static string For(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.Yandex: return Yandex;
                case PlatformId.VKPlay: return VKPlay;
                case PlatformId.RuStore: return RuStore;
                default: return null;
            }
        }
    }
}
