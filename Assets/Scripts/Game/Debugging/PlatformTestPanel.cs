using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Farkle.Platform;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Farkle.Game.Debugging
{
    /// <summary>
    /// Отладочная панель: по кнопке на каждый метод платформенных интерфейсов.
    /// UI строится в коде при старте, чтобы сцена не зависела от ручной разводки ссылок.
    /// Одна и та же сцена собирается под все площадки и показывает, что делает каждая реализация.
    /// </summary>
    public sealed class PlatformTestPanel : MonoBehaviour
    {
        private const int MaxLogLines = 40;
        private const string SaveCounterKey = "farkle.test.counter";

        private IPlatformService _platform;
        private IAdsService _ads;
        private IPurchaseService _purchases;
        private ICloudSaveService _saves;
        private ILeaderboardService _leaderboards;

        private Font _font;
        private Text _logText;
        private readonly List<string> _logLines = new List<string>();
        private readonly List<PurchaseInfo> _lastPending = new List<PurchaseInfo>();

        private Action _onAdOpened;
        private Action _onAdClosed;

        [Inject]
        public void Construct(
            IPlatformService platform,
            IAdsService ads,
            IPurchaseService purchases,
            ICloudSaveService saves,
            ILeaderboardService leaderboards)
        {
            _platform = platform;
            _ads = ads;
            _purchases = purchases;
            _saves = saves;
            _leaderboards = leaderboards;
        }

        private void Start()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildUi();

            _onAdOpened = () => Log("event AdOpened (здесь глушим звук и ставим паузу)");
            _onAdClosed = () => Log("event AdClosed (возвращаем звук и снимаем паузу)");
            _ads.AdOpened += _onAdOpened;
            _ads.AdClosed += _onAdClosed;

            LogStatus();
        }

        private void OnDestroy()
        {
            if (_ads == null)
                return;
            _ads.AdOpened -= _onAdOpened;
            _ads.AdClosed -= _onAdClosed;
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

            AddButton(buttons, "Статус", () => { LogStatus(); return UniTask.CompletedTask; });
            AddButton(buttons, "Авторизация", Authorize);
            AddButton(buttons, "GameReady", () => { _platform.NotifyGameReady(); Log("GameReady отправлен"); return UniTask.CompletedTask; });
            AddButton(buttons, "Gameplay Start", () => { _platform.NotifyGameplayStart(); Log("GameplayStart отправлен"); return UniTask.CompletedTask; });
            AddButton(buttons, "Gameplay Stop", () => { _platform.NotifyGameplayStop(); Log("GameplayStop отправлен"); return UniTask.CompletedTask; });

            AddButton(buttons, "Реклама: доступность", () => { Log($"interstitial={_ads.IsInterstitialAvailable} rewarded={_ads.IsRewardedAvailable}"); return UniTask.CompletedTask; });
            AddButton(buttons, "Показать interstitial", ShowInterstitial);
            AddButton(buttons, "Показать rewarded", ShowRewarded);

            AddButton(buttons, "Магазин: товары", GetProducts);
            AddButton(buttons, "Купить no_ads", () => Purchase("no_ads"));
            AddButton(buttons, "Неподтверждённые покупки", GetPending);
            AddButton(buttons, "Подтвердить все", ConsumeAll);

            AddButton(buttons, "Сохранить", Save);
            AddButton(buttons, "Загрузить", Load);

            AddButton(buttons, "Лидерборд: отправить очки", SubmitScore);
            AddButton(buttons, "Лидерборд: топ 10", GetTop);
            AddButton(buttons, "Лидерборд: моя запись", GetPlayerEntry);

            AddButton(buttons, "Очистить лог", () => { _logLines.Clear(); _logText.text = string.Empty; return UniTask.CompletedTask; });
        }

        private void AddButton(RectTransform parent, string label, Func<UniTask> action)
        {
            var go = new GameObject(label, typeof(RectTransform));
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
            text.text = label;

            button.onClick.AddListener(() => Run(label, action).Forget());
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

        private async UniTaskVoid Run(string label, Func<UniTask> action)
        {
            Log($"<color=#9cf>> {label}</color>");
            try
            {
                await action();
            }
            catch (Exception e)
            {
                Log($"<color=#f66>{label}: {e.GetType().Name}: {e.Message}</color>");
                Debug.LogException(e);
            }
        }

        private void LogStatus()
        {
            Log($"platform={_platform.Platform} initialized={_platform.IsInitialized} lang={_platform.Language} " +
                $"authorized={_platform.IsAuthorized} player={_platform.PlayerName ?? "-"}");
            Log($"purchases={_purchases.IsAvailable} cloudSave={_saves.IsAvailable} leaderboard={_leaderboards.IsAvailable}");
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
            Log($"products: {products.Count}");
            foreach (var p in products)
                Log($"  {p.Id}: {p.Title} за {p.PriceFormatted}");
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
            Log($"pending purchases: {pending.Count}");
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
            Log(json == null ? "no save" : $"loaded: {json}");
        }

        private async UniTask SubmitScore()
        {
            var score = UnityEngine.Random.Range(1000, 15000);
            await _leaderboards.SubmitScoreAsync("main", score);
            Log($"submitted score {score}");
        }

        private async UniTask GetTop()
        {
            var top = await _leaderboards.GetTopAsync("main", 10);
            Log($"top entries: {top.Count}");
            foreach (var e in top)
                Log($"  #{e.Rank} {e.PlayerName}: {e.Score}{(e.IsCurrentPlayer ? " (я)" : "")}");
        }

        private async UniTask GetPlayerEntry()
        {
            var entry = await _leaderboards.GetPlayerEntryAsync("main");
            Log(entry == null ? "player entry: none" : $"player entry: #{entry.Rank} {entry.Score}");
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
