using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farkle.Platform.Yandex
{
    /// <summary>
    /// Лидерборды Яндекс Игр. TODO: ysdk.getLeaderboards(), lb.setLeaderboardScore,
    /// lb.getLeaderboardEntries, lb.getLeaderboardPlayerEntry.
    /// </summary>
    public sealed class YandexLeaderboardService : ILeaderboardService
    {
        private static readonly IReadOnlyList<LeaderboardEntry> Empty = new LeaderboardEntry[0];

        public bool IsAvailable => false;

        public UniTask SubmitScoreAsync(string boardId, long score, CancellationToken cancellationToken = default)
        {
            Debug.LogWarning("[Yandex] SubmitScore: SDK not integrated yet");
            return UniTask.CompletedTask;
        }

        public UniTask<IReadOnlyList<LeaderboardEntry>> GetTopAsync(string boardId, int count, CancellationToken cancellationToken = default)
        {
            return UniTask.FromResult(Empty);
        }

        public UniTask<LeaderboardEntry> GetPlayerEntryAsync(string boardId, CancellationToken cancellationToken = default)
        {
            return UniTask.FromResult<LeaderboardEntry>(null);
        }
    }
}
