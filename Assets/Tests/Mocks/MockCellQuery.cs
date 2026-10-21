using System.Collections.Generic;
using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>GridModel과 "다른 낙하 피스가 있는 칸" 목록만으로 칸 상태를 알려주는 가짜 질의</summary>
    public sealed class MockCellQuery : ICellQuery
    {
        private readonly GridModel _grid;
        private readonly HashSet<GridPos> _pieceCells = new HashSet<GridPos>();

        public MockCellQuery(GridModel grid)
        {
            _grid = grid;
        }

        public void AddPieceCell(int x, int y)
        {
            _pieceCells.Add(new GridPos(x, y));
        }

        public CellState Query(GridPos cell, Piece self)
        {
            if (!_grid.IsInside(cell) || _grid.IsOccupied(cell))
            {
                return CellState.Solid;
            }
            return _pieceCells.Contains(cell) ? CellState.Piece : CellState.Empty;
        }
    }
}
