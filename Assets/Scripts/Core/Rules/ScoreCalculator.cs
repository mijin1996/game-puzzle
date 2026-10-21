using System;

namespace Quadrop.Core
{
    /// <summary>지운 줄 수 · 레벨 · 활성 변 수로 점수를 계산한다.</summary>
    public sealed class ScoreCalculator
    {
        private readonly int[] _lineScores;
        private readonly float[] _sideMultipliers;

        public ScoreCalculator(int[] lineScores, float[] sideMultipliers)
        {
            if (lineScores == null || lineScores.Length == 0)
            {
                throw new ArgumentException("At least one line score is required", nameof(lineScores));
            }
            if (sideMultipliers == null || sideMultipliers.Length == 0)
            {
                throw new ArgumentException("At least one side multiplier is required", nameof(sideMultipliers));
            }

            _lineScores = (int[])lineScores.Clone();
            _sideMultipliers = (float[])sideMultipliers.Clone();
        }

        public int Calculate(int lines, int level, int activeSideCount)
        {
            if (lines <= 0)
            {
                return 0;
            }

            int baseScore = _lineScores[Math.Min(lines, _lineScores.Length) - 1];
            int sideIndex = Math.Max(0, Math.Min(activeSideCount, _sideMultipliers.Length) - 1);
            float score = baseScore * Math.Max(1, level) * _sideMultipliers[sideIndex];
            return (int)MathF.Round(score);
        }
    }
}
