using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Farkle.Core.Localization;
using Farkle.Game.Quality;
using Farkle.Platform;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Farkle.Game.Debugging
{
    /// <summary>
    /// Отладочная панель: по кнопке на каждый метод платформенных интерфейсов.
    /// UI строится в коде при старте, чтобы сцена не зависела от ручной разводки ссылок.
    /// Подписи переводятся: язык приходит из SDK площадки, кнопка смены языка нужна для проверки.
    /// </summary>
    public sealed class PlatformTestPanel : MonoBehaviour
    {
        private const int MaxLogLines = 40;
        private const string SaveCounterKey = "farkle.test.counter";

        /// <summary>Техническое имя лидерборда, должно совпадать с Консолью разработчика.</summary>
        private const string LeaderboardId = "score";

        private IPlatformService _platform;
        private IAdsService _ads;
        private IPurchaseService _purchases;
        private ICloudSaveService _saves;
        private ILeaderboardService _leaderboards;
        private ILocalization _localization;
        private IQualityService _quality;

        private Font _font;
        private Text _logText;
        private readonly List<string> _logLines = new List<string>();
        private readonly List<PurchaseInfo> _lastPending = new List<PurchaseInfo>();

        /// <summary>Подписи, которые нужно перевести заново при смене языка.</summary>
        private readonly List<KeyValuePair<Text, string>> _localizedTexts = new List<KeyValuePair<Text, string>>();

        private Action _onAdOpened;
        private Action _onAdClosed;
        private Action _onLanguageChanged;

        [Inject]
        public void Construct(
            IPlatformService platform,
            IAdsService ads,
            IPurchaseService purchases,
            ICloudSaveService saves,
            ILeaderboardService leaderboards,
            ILocalization localization,
            IQualityService quality)
        {
            _platform = platform;
            _ads = ads;
            _purchases = purchases;
            _saves = saves;
            _leaderboards = leaderboards;
            _localization = localization;
            _quality = quality;
        }

        private void Start()
        {
            // Панель обязана построиться даже при сбое: иначе в билде виден пустой экран
            // без единой подсказки о причине.
            try
            {
                _font = LoadFont();
                BuildUi();
                ReportMissingDependencies();

                if (_ads != null)
                {
                    _onAdOpened = () => Log(Translate("log.adOpened"));
                    _onAdClosed = () => Log(Translate("log.adClosed"));
                    _ads.AdOpened += _onAdOpened;
                    _ads.AdClosed += _onAdClosed;
                }

                // Инициализация площадки асинхронная, язык приходит позже старта сцены.
                if (_localization != null)
                {
                    _onLanguageChanged = OnLanguageChanged;
                    _localization.LanguageChanged += _onLanguageChanged;
                }

                LogStatus();
            }
            catch (Exception e)
            {
                Debug.LogError("[TestPanel] Build failed: " + e);
                Debug.LogException(e);
                ShowFatalError();
            }
        }

        /// <summary>
        /// Шрифт интерфейса. Свой TTF обязателен: встроенный шрифт Unity
        /// в WebGL-сборке не рисует кириллицу, потому что в браузере нет системных шрифтов.
        /// Встроенный остаётся запасным вариантом на случай, если ресурс не найден.
        /// </summary>
        private static Font LoadFont()
        {
            var font = Resources.Load<Font>("Fonts/Roboto-Regular");
            if (font != null)
                return font;

            Debug.LogError("[TestPanel] Fonts/Roboto-Regular not found, Cyrillic text will be invisible in WebGL");

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return font;
        }

        /// <summary>Перевод, устойчивый к отсутствию зависимости: без неё показываем сам ключ.</summary>
        private string Translate(string key)
        {
            return _localization != null ? _localization.Get(key) : key;
        }

        /// <summary>Пишет в лог, какие зависимости не пришли из контейнера.</summary>
        private void ReportMissingDependencies()
        {
            var missing = new List<string>();
            if (_platform == null) missing.Add(nameof(IPlatformService));
            if (_ads == null) missing.Add(nameof(IAdsService));
            if (_purchases == null) missing.Add(nameof(IPurchaseService));
            if (_saves == null) missing.Add(nameof(ICloudSaveService));
            if (_leaderboards == null) missing.Add(nameof(ILeaderboardService));
            if (_localization == null) missing.Add(nameof(ILocalization));
            if (_quality == null) missing.Add(nameof(IQualityService));

            if (missing.Count == 0)
                return;

            var message = "Dependency injection failed: " + string.Join(", ", missing);
            Debug.LogError("[TestPanel] " + message);
            Log($"<color=#f66>{message}</color>");
            ShowFatalError();
        }

        /// <summary>Красная полоса сверху: видна даже когда шрифт не загрузился.</summary>
        private void ShowFatalError()
        {
            var rect = CreateRect("FatalError", (RectTransform)transform, new Vector2(0f, 0.96f), Vector2.one);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.8f, 0.15f, 0.15f, 1f);
            image.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (_ads != null)
            {
                _ads.AdOpened -= _onAdOpened;
                _ads.AdClosed -= _onAdClosed;
            }

            if (_localization != null)
                _localization.LanguageChanged -= _onLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            foreach (var pair in _localizedTexts)
            {
                if (pair.Key != null)
                    pair.Key.text = Translate(pair.Value);
            }

            Log($"{Translate("log.languageDetected")}: {_localization.Language}");
        }

        // ---------- UI ----------

        private void BuildUi()
        {
            var root = (RectTransform)transform;
            Stretch(root);

            var buttons = CreateRect("Buttons", root, new Vector2(0f, 0.42f), Vector2.one);
            var grid = buttons.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(300f, 64f);
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(16, 16, 16, 16);
            grid.childAlignment = TextAnchor.UpperLeft;

            var logArea = CreateRect("Log", root, Vector2.zero, new Vector2(1f, 0.42f));
            var background = logArea.gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            var logTextRect = CreateRect("Text", logArea, Vector2.zero, Vector2.one);
            logTextRect.offsetMin = new Vector2(16f, 12f);
            logTextRect.offsetMax = new Vector2(-16f, -12f);
            _logText = logTextRect.gameObject.AddComponent<Text>();
            _logText.font = _font;
            _logText.fontSize = 20;
            _logText.color = Color.white;
            _logText.alignment = TextAnchor.LowerLeft;
            _logText.supportRichText = true;
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logText.verticalOverflow = VerticalWrapMode.Truncate;

            AddButton(buttons, "btn.status", () => { LogStatus(); return UniTask.CompletedTask; });
            AddButton(buttons, "btn.authorize", Authorize);
            AddButton(buttons, "btn.gameReady", () => { _platform.NotifyGameReady(); Log(Translate("log.gameReadySent")); return UniTask.CompletedTask; });
            AddButton(buttons, "btn.gameplayStart", () => { _platform.NotifyGameplayStart(); Log(Translate("log.gameplayStartSent")); return UniTask.CompletedTask; });
            AddButton(buttons, "btn.gameplayStop", () => { _platform.NotifyGameplayStop(); Log(Translate("log.gameplayStopSent")); return UniTask.CompletedTask; });

            AddButton(buttons, "btn.adsStatus", () => { Log($"interstitial={_ads.IsInterstitialAvailable} rewarded={_ads.IsRewardedAvailable}"); return UniTask.CompletedTask; });
            AddButton(buttons, "btn.interstitial", ShowInterstitial);
            AddButton(buttons, "btn.rewarded", ShowRewarded);

            AddButton(buttons, "btn.products", GetProducts);
            AddButton(buttons, "btn.buy", () => Purchase("no_ads"));
            AddButton(buttons, "btn.pending", GetPending);
            AddButton(buttons, "btn.consume", ConsumeAll);

            AddButton(buttons, "btn.save", Save);
            AddButton(buttons, "btn.load", Load);

            AddButton(buttons, "btn.submitScore", SubmitScore);
            AddButton(buttons, "btn.top", GetTop);
            AddButton(buttons, "btn.myEntry", GetPlayerEntry);

            AddButton(buttons, "btn.language", SwitchLanguage);
            AddButton(buttons, "btn.quality", SwitchQuality);
            AddButton(buttons, "btn.clearLog", () => { _logLines.Clear(); _logText.text = string.Empty; return UniTask.CompletedTask; });
        }

        private void AddButton(RectTransform parent, string labelKey, Func<UniTask> action)
        {
            var go = new GameObject(labelKey, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.45f, 0.8f, 1f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var textRect = CreateRect("Label", (RectTransform)go.transform, Vector2.zero, Vector2.one);
            var text = textRect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.text = Translate(labelKey);

            _localizedTexts.Add(new KeyValuePair<Text, string>(text, labelKey));

            button.onClick.AddListener(() => Run(labelKey, action).Forget());
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // ---------- Actions ----------

        private async UniTaskVoid Run(string labelKey, Func<UniTask> action)
        {
            Log($"<color=#9cf>> {Translate(labelKey)}</color>");
            try
            {
                await action();
            }
            catch (Exception e)
            {
                Log($"<color=#f66>{e.GetType().Name}: {e.Message}</color>");
                Debug.LogException(e);
            }
        }

        /// <summary>Ручное переключение языка нужно, чтобы проверить перевод без смены языка площадки.</summary>
        private UniTask SwitchLanguage()
        {
            if (_localization == null)
                return UniTask.CompletedTask;

            var supported = Localization.Supported;
            var index = 0;
            for (var i = 0; i < supported.Count; i++)
            {
                if (supported[i] == _localization.Language)
                {
                    index = i;
                    break;
                }
            }

            _localization.SetLanguage(supported[(index + 1) % supported.Count]);
            return UniTask.CompletedTask;
        }

        /// <summary>Перебирает уровни качества по кругу, чтобы проверить переключение Quality Level.</summary>
        private UniTask SwitchQuality()
        {
            if (_quality == null)
                return UniTask.CompletedTask;

            var tiers = (QualityTier[])Enum.GetValues(typeof(QualityTier));
            var next = tiers[((int)_quality.Tier + 1) % tiers.Length];
            _quality.SetTier(next);
            LogQuality();
            return UniTask.CompletedTask;
        }

        private void LogQuality()
        {
            var level = QualitySettings.GetQualityLevel();
            var names = QualitySettings.names;
            var levelName = level >= 0 && level < names.Length ? names[level] : "?";
            Log($"{Translate("log.quality")}: device={_quality.Device} recommended={_quality.Recommended} " +
                $"tier={_quality.Tier} level={levelName} screen={Screen.width}x{Screen.height}");
        }

        private void LogStatus()
        {
            if (_platform == null || _localization == null)
            {
                Log("<color=#f66>Status is unavailable: services were not injected</color>");
                return;
            }

            Log($"platform={_platform.Platform} initialized={_platform.IsInitialized} " +
                $"sdkLang={_platform.Language} uiLang={_localization.Language} " +
                $"authorized={_platform.IsAuthorized} player={_platform.PlayerName ?? "-"}");
            Log($"purchases={_purchases.IsAvailable} cloudSave={_saves.IsAvailable} leaderboard={_leaderboards.IsAvailable}");

            if (_quality != null)
                LogQuality();
        }

        private async UniTask Authorize()
        {
            var ok = await _platform.AuthorizeAsync();
            Log($"authorized={ok} id={_platform.PlayerId ?? "-"} name={_platform.PlayerName ?? "-"}");
        }

        private async UniTask ShowInterstitial()
        {
            var shown = await _ads.ShowInterstitialAsync();
            Log($"interstitial shown={shown}");
        }

        private async UniTask ShowRewarded()
        {
            var result = await _ads.ShowRewardedAsync("test_panel");
            Log($"rewarded result={result}");
        }

        private async UniTask GetProducts()
        {
            var products = await _purchases.GetProductsAsync();
            Log($"{Translate("log.products")}: {products.Count}");
            foreach (var p in products)
                Log($"  {p.Id}: {p.Title} - {p.PriceFormatted}");
        }

        private async UniTask Purchase(string productId)
        {
            var result = await _purchases.PurchaseAsync(productId);
            Log(result.Success
                ? $"purchased {result.Purchase.ProductId}, token={Short(result.Purchase.PurchaseToken)}"
                : $"purchase failed: {result.Error}");
        }

        private async UniTask GetPending()
        {
            var pending = await _purchases.GetPendingPurchasesAsync();
            _lastPending.Clear();
            _lastPending.AddRange(pending);
            Log($"{Translate("log.pending")}: {pending.Count}");
            foreach (var p in pending)
                Log($"  {p.ProductId} token={Short(p.PurchaseToken)}");
        }

        private async UniTask ConsumeAll()
        {
            if (_lastPending.Count == 0)
                await GetPending();

            foreach (var p in _lastPending)
            {
                await _purchases.ConsumeAsync(p.PurchaseToken);
                Log($"consumed {p.ProductId}");
            }
            _lastPending.Clear();
        }

        private async UniTask Save()
        {
            var counter = PlayerPrefs.GetInt(SaveCounterKey, 0) + 1;
            PlayerPrefs.SetInt(SaveCounterKey, counter);
            var json = $"{{\"counter\":{counter},\"savedAt\":\"{DateTime.Now:HH:mm:ss}\"}}";
            await _saves.SaveAsync(json);
            Log($"saved: {json}");
        }

        private async UniTask Load()
        {
            var json = await _saves.LoadAsync();
            Log(json == null ? Translate("log.noSave") : $"loaded: {json}");
        }

        private async UniTask SubmitScore()
        {
            var score = UnityEngine.Random.Range(1000, 15000);
            await _leaderboards.SubmitScoreAsync(LeaderboardId, score);
            Log($"submitted score {score}");
        }

        private async UniTask GetTop()
        {
            var top = await _leaderboards.GetTopAsync(LeaderboardId, 10);
            Log($"{Translate("log.topEntries")}: {top.Count}");
            foreach (var e in top)
                Log($"  #{e.Rank} {e.PlayerName}: {e.Score}{(e.IsCurrentPlayer ? $" ({Translate("log.me")})" : "")}");
        }

        private async UniTask GetPlayerEntry()
        {
            var entry = await _leaderboards.GetPlayerEntryAsync(LeaderboardId);
            Log(entry == null ? Translate("log.noEntry") : $"#{entry.Rank} {entry.Score}");
        }

        // ---------- Log ----------

        private void Log(string message)
        {
            Debug.Log("[TestPanel] " + message);

            _logLines.Add($"<color=#888>{DateTime.Now:HH:mm:ss}</color> {message}");
            if (_logLines.Count > MaxLogLines)
                _logLines.RemoveRange(0, _logLines.Count - MaxLogLines);

            if (_logText == null)
                return;

            var sb = new StringBuilder();
            foreach (var line in _logLines)
                sb.AppendLine(line);
            _logText.text = sb.ToString();
        }

        private static string Short(string token)
        {
            return string.IsNullOrEmpty(token) ? "-" : token.Length <= 8 ? token : token.Substring(0, 8) + "...";
        }
    }
}
