using System;

namespace Quadrop.Game
{
    /// <summary>한 판의 점수 · 지운 줄 수 · 레벨</summary>
    public sealed class GameStats
    {
        private readonly int _linesPerLevel;

        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Level => 1 + Lines / _linesPerLevel;

        public GameStats(int linesPerLevel)
        {
            if (linesPerLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(linesPerLevel));
            }
            _linesPerLevel = linesPerLevel;
        }

        internal void AddClear(int lines, int points)
        {
            if (lines < 0 || points < 0)
            {
                throw new ArgumentOutOfRangeException(lines < 0 ? nameof(lines) : nameof(points));
            }
            Lines += lines;
            Score += points;
        }

        internal void Reset()
        {
            Score = 0;
            Lines = 0;
        }
    }
}
