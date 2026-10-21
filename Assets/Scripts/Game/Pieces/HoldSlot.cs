using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>모든 변이 공유하는 홀드 칸. 피스가 하나 고정될 때마다 한 번 쓸 수 있다.</summary>
    internal sealed class HoldSlot
    {
        public TetrominoType Type { get; private set; } = TetrominoType.None;
        public bool CanUse { get; private set; } = true;

        /// <summary>current를 맡기고, 원래 들어 있던 종류를 released로 돌려준다 (비어 있었으면 None).</summary>
        public bool TryHold(TetrominoType current, out TetrominoType released)
        {
            released = TetrominoType.None;
            if (!CanUse || current == TetrominoType.None)
            {
                return false;
            }

            released = Type;
            Type = current;
            CanUse = false;
            return true;
        }

        public void AllowUse()
        {
            CanUse = true;
        }

        public void Clear()
        {
            Type = TetrominoType.None;
            CanUse = true;
        }
    }
}
