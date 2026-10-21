using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>항상 같은 값을 돌려주는 랜덤 (범위를 넘으면 maxExclusive - 1)</summary>
    public sealed class FixedRandomSource : IRandomSource
    {
        private readonly int _value;

        public FixedRandomSource(int value)
        {
            _value = value;
        }

        public int Next(int maxExclusive)
        {
            return _value < maxExclusive ? _value : maxExclusive - 1;
        }
    }
}
