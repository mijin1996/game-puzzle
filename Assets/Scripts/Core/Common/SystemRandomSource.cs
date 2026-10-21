using System;

namespace Quadrop.Core
{
    /// <summary>System.Random을 감싼 실제 게임용 랜덤</summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }
            return _random.Next(maxExclusive);
        }
    }
}
