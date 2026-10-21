using System;

namespace Quadrop.Core
{
    /// <summary>4개 변의 로컬 좌표계를 한 번만 만들어 두고, 피스 셀의 판 좌표를 계산한다.</summary>
    public sealed class BoardFrames
    {
        private readonly SideFrame[] _frames;

        public int BoardSize { get; }

        public BoardFrames(int boardSize)
        {
            if (boardSize < GridModel.MinSize)
            {
                throw new ArgumentOutOfRangeException(nameof(boardSize));
            }

            BoardSize = boardSize;
            _frames = new SideFrame[SideExtensions.Count];
            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = new SideFrame((Side)i, boardSize);
            }
        }

        public SideFrame FrameOf(Side side)
        {
            return _frames[(int)side];
        }

        /// <summary>피스를 (u, v, rotation)에 놓았을 때 cellIndex번째 셀의 판 좌표</summary>
        public GridPos CellOf(Piece piece, int u, int v, int rotation, int cellIndex)
        {
            GridPos local = TetrominoShapes.GetCell(piece.Type, rotation, cellIndex);
            return _frames[(int)piece.Side].ToGrid(u + local.X, v + local.Y);
        }

        public GridPos CellOf(Piece piece, int cellIndex)
        {
            return CellOf(piece, piece.U, piece.V, piece.Rotation, cellIndex);
        }
    }
}
