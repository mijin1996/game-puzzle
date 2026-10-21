using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>항상 같은 피스만 내보내는 순서 (결과를 예측 가능하게)</summary>
    public sealed class FixedPieceSequence : IPieceSequence
    {
        private readonly TetrominoType _type;

        public int NextCount { get; private set; }

        public FixedPieceSequence(TetrominoType type)
        {
            _type = type;
        }

        public TetrominoType Next()
        {
            NextCount++;
            return _type;
        }

        public TetrominoType Peek(int index)
        {
            return _type;
        }

        public void Reset()
        {
            NextCount = 0;
        }
    }
}
