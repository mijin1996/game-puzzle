namespace Quadrop.Core
{
    /// <summary>칸 상태. 값이 클수록 더 강하게 막힌다.</summary>
    public enum CellState
    {
        Empty = 0,
        Piece = 1,
        Solid = 2
    }
}
