using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>게임오버 때 최고 점수를 갱신해 저장한다.</summary>
    public sealed class HighScoreTracker : IDisposable
    {
        private readonly GameEvents _events;
        private readonly GameStats _stats;
        private readonly IHighScoreRepository _repository;

        public int Best { get; private set; }
        public string LastError { get; private set; }

        public event Action BestChanged;

        public HighScoreTracker(GameEvents events, GameStats stats, IHighScoreRepository repository)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _events.StateChanged += OnStateChanged;
        }

        /// <summary>저장된 값을 읽는다. 처음 하는 게임이면 저장된 값이 없으므로 0으로 시작한다.</summary>
        public void Load()
        {
            Result<int> result = _repository.Load();
            Best = result.IsSuccess ? Math.Max(0, result.Value) : 0;
            LastError = result.IsSuccess ? null : result.Error;
            BestChanged?.Invoke();
        }

        public void Dispose()
        {
            _events.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameStateId state)
        {
            if (state != GameStateId.GameOver || _stats.Score <= Best)
            {
                return;
            }

            Best = _stats.Score;
            Result<bool> saved = _repository.Save(Best);
            LastError = saved.IsSuccess ? null : saved.Error;
            BestChanged?.Invoke();
        }
    }
}
