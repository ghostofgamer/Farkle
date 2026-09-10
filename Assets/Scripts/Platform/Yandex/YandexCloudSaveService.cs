using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.Yandex
{
    /// <summary>
    /// Облачное сохранение Яндекс Игр. TODO: player.setData / player.getData.
    /// Требует авторизованного игрока, иначе данные хранятся только локально.
    /// </summary>
    public sealed class YandexCloudSaveService : ICloudSaveService
    {
        public bool IsAvailable => false;

        public UniTask SaveAsync(string json, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] Save: SDK not integrated yet");
            return UniTask.CompletedTask;
        }

        public UniTask<string> LoadAsync(CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] Load: SDK not integrated yet");
            return UniTask.FromResult<string>(null);
        }
    }
}
