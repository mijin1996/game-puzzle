namespace Quadrop.Core
{
    /// <summary>화면 쪽에 넘겨주는 읽기 전용 판 (값을 바꿀 수 없게)</summary>
    public interface IReadOnlyGrid
    {
        int Size { get; }

        /// <summary>0 = 빈칸, 1~7 = 고정된 블록의 TetrominoType</summary>
        byte Get(int x, int y);
    }
}
