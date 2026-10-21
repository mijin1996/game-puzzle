namespace Quadrop.Core
{
    /// <summary>다음 피스 순서. 모든 변이 하나의 순서를 공유한다.</summary>
    public interface IPieceSequence
    {
        TetrominoType Next();

        /// <summary>index번째 뒤의 피스를 꺼내지 않고 본다 (0 = 바로 다음).</summary>
        TetrominoType Peek(int index);

        void Reset();
    }
}
