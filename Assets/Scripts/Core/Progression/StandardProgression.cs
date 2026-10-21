using System;

namespace Quadrop.Core
{
    /// <summary>STANDARD 모드: 한 방향으로 시작해서 정해진 줄 수마다 변이 하나씩 열린다.</summary>
    public sealed class StandardProgression : IProgressionRule
    {
        private readonly int[] _thresholds;
        private readonly Side[] _order;

        public StandardProgression(int[] thresholds, Side[] order)
        {
            if (thresholds == null || order == null)
            {
                throw new ArgumentNullException(thresholds == null ? nameof(thresholds) : nameof(order));
            }
            if (thresholds.Length == 0 || thresholds.Length != order.Length)
            {
                throw new ArgumentException("Thresholds and order must have the same non-zero length");
            }
            if (thresholds[0] != 0)
            {
                throw new ArgumentException("The first side must be open from the start", nameof(thresholds));
            }
            for (int i = 1; i < thresholds.Length; i++)
            {
                if (thresholds[i] < thresholds[i - 1])
                {
                    throw new ArgumentException("Thresholds must be ascending", nameof(thresholds));
                }
            }

            _thresholds = (int[])thresholds.Clone();
            _order = (Side[])order.Clone();
        }

        public SideSet ActiveSidesFor(int totalLines)
        {
            SideSet sides = SideSet.None;
            for (int i = 0; i < _thresholds.Length; i++)
            {
                if (totalLines >= _thresholds[i])
                {
                    sides = sides.With(_order[i]);
                }
            }
            return sides;
        }

        public int NextUnlockAt(int totalLines)
        {
            for (int i = 0; i < _thresholds.Length; i++)
            {
                if (_thresholds[i] > totalLines)
                {
                    return _thresholds[i];
                }
            }
            return -1;
        }
    }
}
