# Farkle: структура проекта и правила

Игра Фаркл (кости) на Unity 6000.3 (URP). Одна кодовая база, три площадки:
Яндекс Игры (WebGL), VK Play (WebGL), RuStore (Android). Площадка выбирается на этапе сборки.

## Стек и соглашения

- **Async: только UniTask.** Корутины не используются. Все асинхронные методы возвращают `UniTask` / `UniTask<T>`
  и принимают `CancellationToken`. Fire-and-forget только через `.Forget()`.
- **DI: Zenject (Extenject из Asset Store, лежит в `Assets`, сборка `Zenject`).** Не ставить через OpenUPM или git, чтобы не было двух копий. Зависимости через конструктор. Никаких `FindObjectOfType`, синглтонов и статических сервисов.
  Единственный `ProjectContext` живёт в `Assets/Resources/ProjectContext.prefab` (создать: меню `Farkle/Setup/Create ProjectContext`).
- **Площадка выбирается define:** `FARKLE_YANDEX`, `FARKLE_VKPLAY`, `FARKLE_RUSTORE`. Ровно один в билде. Без define работает Stub.
- **В редакторе всегда Stub**, независимо от define. SDK площадок работают только в реальном билде.
- Игровой код (Core, Game) работает с площадкой **только через интерфейсы** из `Farkle.Platform.Abstractions`.
  Прямые ссылки на Yandex/VKPlay/RuStore из игрового кода запрещены.
- **Stripping Level High + `Assets/link.xml`.** Любая новая сборка, чьи классы биндятся в Zenject или вызываются из JS/Android через reflection, добавляется в `link.xml` с `preserve="all"`.
- Каждое изменение проекта записывается в `README.md` (раздел «Журнал изменений»). Этот файл держим актуальным при изменении структуры.
- Комментарии и документация на русском, идентификаторы на английском.

## Структура

```
Assets/
  Scripts/
    Core/                 Farkle.Core.asmdef       правила Фаркла, чистый C#, без UnityEngine
    Game/                 Farkle.Game.asmdef       сцены, UI, презентация; ссылается на Core и Abstractions
      Debugging/PlatformTestPanel.cs  отладочная панель: кнопка на каждый метод платформенных интерфейсов, UI строится в коде
    Platform/
      Abstractions/       Farkle.Platform.Abstractions.asmdef
                            IPlatformService     жизненный цикл SDK, язык, авторизация, GameReady/GameplayStart/Stop
                            IAdsService          interstitial, rewarded, события AdOpened/AdClosed для паузы и звука
                            IPurchaseService     каталог, покупка, pending-покупки, consume
                            ICloudSaveService    один JSON-блоб на игрока
                            ILeaderboardService  submit, top, запись игрока
                            PlatformId, PlatformDefines
      Stub/               Farkle.Platform.Stub.asmdef     заглушки: редактор и билды без SDK (PlayerPrefs, мгновенный успех)
      Yandex/             Farkle.Platform.Yandex.asmdef   define FARKLE_YANDEX, платформы WebGL+Editor
        Plugins/            .jslib Яндекс SDK (пока пусто)
      VKPlay/             Farkle.Platform.VKPlay.asmdef   define FARKLE_VKPLAY, платформы WebGL+Editor
        Plugins/            .jslib VK Bridge (пока пусто)
      RuStore/            Farkle.Platform.RuStore.asmdef  define FARKLE_RUSTORE, платформы Android+Editor
        Plugins/Android/    .aar RuStore Billing и рекламной сети (пока пусто)
      Installers/         Farkle.Platform.Installers.asmdef
                            PlatformInstaller    единственное место выбора реализации по define
                            PlatformInitializer  IInitializable, запускает InitializeAsync площадки
    Editor/               Farkle.Editor.asmdef
                            PlatformSwitcher         меню Farkle/Platform: target, defines, шаблон, плагины
                            PlatformPluginToggler    включает .jslib/.aar только активной площадки
                            PlatformBuildPreprocessor проверка перед любой сборкой, падает при несоответствии
                            BuildScript              меню Farkle/Build и CLI-точки входа, результат в Builds/<площадка>
                            ProjectContextCreator    меню Farkle/Setup/Create ProjectContext
                            PlatformTestSceneCreator меню Farkle/Setup/Create Platform Test Scene, создаёт Scenes/PlatformTest.unity
                            PlatformTargets          соответствие площадка -> BuildTarget, шаблон, папка плагинов
  WebGLTemplates/
    Yandex/index.html     шаблон для Яндекс Игр (подключение /sdk.js закомментировано до интеграции)
    VKPlay/index.html     шаблон для VK Play (подключение vk-bridge закомментировано до интеграции)
  Resources/
    ProjectContext.prefab Zenject ProjectContext с PlatformInstaller
  Scenes/
    PlatformTest.unity    тестовая сцена с кнопками, первая в Build Settings, пока нет игровых сцен
    SampleScene.unity     остаток шаблона URP
  link.xml                защита сборок Farkle.* и UniTask от стриппинга
Packages/manifest.json    UniTask (git). Zenject не здесь, а в Assets из Asset Store
```

## Как переключить площадку

Меню `Farkle/Platform/<площадка>`. Скрипт:
1. переключает активный Build Target (WebGL или Android),
2. ставит define площадки на нужный target и снимает FARKLE_* с остальных,
3. выставляет WebGL-шаблон (`PROJECT:Yandex` / `PROJECT:VKPlay`),
4. включает нативные плагины только этой площадки.

`Farkle/Platform/Show Current` печатает текущее состояние в консоль.

## Как собрать

Меню `Farkle/Build/<площадка>` или CLI:

```
Unity -batchmode -quit -projectPath . -buildTarget WebGL   -executeMethod Farkle.Editor.BuildScript.BuildYandex
Unity -batchmode -quit -projectPath . -buildTarget WebGL   -executeMethod Farkle.Editor.BuildScript.BuildVKPlay
Unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod Farkle.Editor.BuildScript.BuildRuStore
```

Сборка через обычное окно Build тоже работает: `PlatformBuildPreprocessor` проверит, что defines, target,
шаблон и плагины согласованы, и остановит сборку с подсказкой, если нет.

## Как добавить SDK площадки

1. Нативные файлы (.jslib, .aar, .jar) класть **только** в `Platform/<площадка>/Plugins`. Никогда в `Assets/Plugins`.
2. C#-обёртки класть в `Platform/<площадка>/`, они компилируются только под своим define.
3. Заменить TODO-реализации сервисов в `Platform/<площадка>/` на вызовы SDK, сигнатуры интерфейсов не менять.
4. Для web-площадок раскомментировать подключение SDK в `WebGLTemplates/<площадка>/index.html`.
5. Android-зависимости версионировать явно (EDM4U или `mainTemplate.gradle`), чтобы конфликты ловились на gradle-резолве.
6. После добавления запустить `Farkle/Platform/<площадка>`, чтобы плагины получили правильные настройки импорта.

## Что не трогать

- `Assets/TutorialInfo`, `Assets/Readme.asset`: остатки шаблона Unity, удалить при первой чистке.
- `Far.sln`: дубликат `Farkle.sln`, можно удалить.
