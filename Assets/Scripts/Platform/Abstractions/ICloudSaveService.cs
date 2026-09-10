using System.Threading;
using Cysharp.Threading.Tasks;

namespace Farkle.Platform
{
    /// <summary>
    /// Облачное сохранение. Один JSON-блоб на игрока.
    /// Сериализацию делает игра, площадка только хранит строку.
    /// </summary>
    public interface ICloudSaveService
    {
        bool IsAvailable { get; }

        UniTask SaveAsync(string json, CancellationToken cancellationToken = default);

        /// <summary>Возвращает null, если сохранения ещё нет.</summary>
        UniTask<string> LoadAsync(CancellationToken cancellationToken = default);
    }
}
