using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>고정 블록(GridModel)과 다른 변의 낙하 피스를 함께 보고 칸 상태를 알려준다.</summary>
    internal sealed class BoardCollision : ICellQuery
    {
        private readonly GridModel _grid;
        private readonly BoardFrames _frames;
        private readonly SpawnerSet _spawners;

        public BoardCollision(GridModel grid, BoardFrames frames, SpawnerSet spawners)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _frames = frames ?? throw new ArgumentNullException(nameof(frames));
            _spawners = spawners ?? throw new ArgumentNullException(nameof(spawners));
        }

        public CellState Query(GridPos cell, Piece self)
        {
            if (!_grid.IsInside(cell) || _grid.IsOccupied(cell))
            {
                return CellState.Solid;
            }

            for (int i = 0; i < SideExtensions.Count; i++)
            {
                if (!_spawners.TryGetPiece((Side)i, out Piece other) || ReferenceEquals(other, self))
                {
                    continue;
                }
                if (Occupies(other, cell))
                {
                    return CellState.Piece;
                }
            }
            return CellState.Empty;
        }

        private bool Occupies(Piece piece, GridPos cell)
        {
            for (int i = 0; i < TetrominoShapes.CellCount; i++)
            {
                if (_frames.CellOf(piece, i) == cell)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
