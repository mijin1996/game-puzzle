using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>낙하 피스를 판에 고정한다.</summary>
    internal sealed class PieceLocker
    {
        private readonly GridModel _grid;
        private readonly BoardFrames _frames;
        private readonly SpawnerSet _spawners;
        private readonly HoldSlot _hold;
        private readonly GameEvents _events;

        public PieceLocker(GridModel grid, BoardFrames frames, SpawnerSet spawners, HoldSlot hold, GameEvents events)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _frames = frames ?? throw new ArgumentNullException(nameof(frames));
            _spawners = spawners ?? throw new ArgumentNullException(nameof(spawners));
            _hold = hold ?? throw new ArgumentNullException(nameof(hold));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Lock(Side side)
        {
            if (!_spawners.TryGetPiece(side, out Piece piece))
            {
                return;
            }

            for (int i = 0; i < TetrominoShapes.CellCount; i++)
            {
                GridPos cell = _frames.CellOf(piece, i);
                if (_grid.IsInside(cell))
                {
                    _grid.Set(cell, (byte)piece.Type);
                }
            }

            _spawners.Remove(side);
            _hold.AllowUse();
            _events.RaisePieceLocked(side);
            _events.RaiseBoardChanged();
        }
    }
}
