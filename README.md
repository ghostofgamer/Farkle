# Farkle

Игра Фаркл (кости) на Unity 6000.3 для Яндекс Игр, VK Play и RuStore.
Структура проекта, стек и правила: [AGENTS.md](AGENTS.md).

## Журнал изменений

### 2026-09-10

- Заложена архитектура под три площадки: игровой код работает с площадкой только через интерфейсы
  `IPlatformService`, `IAdsService`, `IPurchaseService`, `ICloudSaveService`, `ILeaderboardService`.
- Созданы сборки (asmdef): `Farkle.Core`, `Farkle.Game`, `Farkle.Platform.Abstractions`, `Farkle.Platform.Stub`,
  `Farkle.Platform.Yandex`, `Farkle.Platform.VKPlay`, `Farkle.Platform.RuStore`, `Farkle.Platform.Installers`, `Farkle.Editor`.
- Реализации площадок ограничены define: `FARKLE_YANDEX`, `FARKLE_VKPLAY`, `FARKLE_RUSTORE`. Пока это заглушки с TODO
  под конкретные вызовы SDK, билд собирается без SDK.
- `Stub`-реализация для редактора: реклама эмулируется задержкой, сохранения в PlayerPrefs, покупки всегда успешны.
- Zenject: `PlatformInstaller` выбирает реализацию по define, в редакторе всегда Stub. `PlatformInitializer` запускает SDK.
- Редакторские инструменты: меню `Farkle/Platform` (переключение площадки), `Farkle/Build` (сборка в `Builds/`),
  `Farkle/Setup/Create ProjectContext`. Pre-build проверка останавливает сборку при рассогласовании defines, target,
  WebGL-шаблона и плагинов.
- WebGL-шаблоны `Yandex` и `VKPlay` с закомментированным подключением SDK.
- В `Packages/manifest.json` добавлен UniTask 2.5.11 (git).
- Extenject с OpenUPM (9.2.0-stcf3, 2020 год) удалён вместе с реестром OpenUPM. Zenject ставится вручную
  из Asset Store (Extenject Dependency Injection IOC) в `Assets`. Имя сборки то же, `Zenject`, ссылки в asmdef не меняются.
- Добавлены `AGENTS.md` (структура и правила) и этот журнал.
- Zenject (Extenject 9.2.0) установлен из Asset Store в `Assets/Plugins/Zenject`.
- Создан `Assets/Resources/ProjectContext.prefab` с `PlatformInstaller`.
- Первая WebGL-сборка под Яндекс Игры собралась: `Builds/Yandex`, 14 МБ, шаблон `PROJECT:Yandex`, define `FARKLE_YANDEX`.
- В `.gitignore` добавлена папка `.idea/`.
- Включён Managed Stripping Level High. Добавлен `Assets/link.xml`, который сохраняет все сборки `Farkle.*` и UniTask
  от стриппинга, иначе Zenject не сможет создавать объекты через reflection в IL2CPP-билде.
- Тестовая сцена `Scenes/PlatformTest.unity` (меню `Farkle/Setup/Create Platform Test Scene`): кнопка на каждый метод
  платформенных интерфейсов, лог на экране. `PlatformTestPanel` строит UI в коде, зависимости получает через Zenject.
  Сцена ставится первой в Build Settings.
- Вторая WebGL-сборка под VK Play собралась: `Builds/VKPlay`, 13 МБ, шаблон `PROJECT:VKPlay`, define `FARKLE_VKPLAY`.
  В билд попала сборка `Farkle.Platform.VKPlay`, сборки Yandex в нём нет. Механизм переключения площадок подтверждён.
