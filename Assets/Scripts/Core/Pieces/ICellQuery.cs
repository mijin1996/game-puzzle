namespace Quadrop.Core
{
    /// <summary>"이 칸에 들어갈 수 있나" 질의. 고정 블록과 다른 낙하 피스를 모두 아는 쪽이 구현한다.</summary>
    public interface ICellQuery
    {
        /// <summary>판 밖이나 고정 블록은 Solid, self가 아닌 낙하 피스는 Piece, 나머지는 Empty</summary>
        CellState Query(GridPos cell, Piece self);
    }
}
