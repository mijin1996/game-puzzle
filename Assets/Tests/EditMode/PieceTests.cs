using System.Collections.Generic;
using NUnit.Framework;
using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>판 8, 레인 폭 8 (레인 = 로컬 u 0~7, v 4~7)에서 피스 이동 규칙을 검사한다.</summary>
    public sealed class PieceTests
    {
        private const int Size = 8;
        private const int LaneFloor = Size / 2;

        private GridModel _grid;
        private MockCellQuery _query;
        private LaneLayout _lanes;
        private PieceMover _mover;

        [SetUp]
        public void SetUp()
        {
            _grid = MockGrids.Empty(Size);
            _query = new MockCellQuery(_grid);
            _lanes = new LaneLayout(new BoardFrames(Size), Size);
            _mover = new PieceMover(_query, _lanes);
        }

        [Test]
        public void Shapes_FourRotations_ReturnToSpawnShape()
        {
            for (int t = 0; t < TetrominoShapes.TypeCount; t++)
            {
                TetrominoType type = TetrominoShapes.FromIndex(t);
                for (int i = 0; i < TetrominoShapes.CellCount; i++)
                {
                    Assert.AreEqual(TetrominoShapes.GetCell(type, 0, i), TetrominoShapes.GetCell(type, 4, i), type.ToString());
                }
            }
        }

        [Test]
        public void Shapes_AllRotations_StayInsideBox()
        {
            for (int t = 0; t < TetrominoShapes.TypeCount; t++)
            {
                TetrominoType type = TetrominoShapes.FromIndex(t);
                int box = TetrominoShapes.BoxSize(type);
                for (int r = 0; r < TetrominoShapes.RotationCount; r++)
                {
                    HashSet<GridPos> distinct = new HashSet<GridPos>();
                    for (int i = 0; i < TetrominoShapes.CellCount; i++)
                    {
                        GridPos cell = TetrominoShapes.GetCell(type, r, i);
                        Assert.That(cell.X >= 0 && cell.X < box && cell.Y >= 0 && cell.Y < box, $"{type} r{r}");
                        distinct.Add(cell);
                    }
                    Assert.AreEqual(TetrominoShapes.CellCount, distinct.Count);
                }
            }
        }

        [Test]
        public void TryMove_EmptyBelow_Moves()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, 6);
            Assert.AreEqual(MoveResult.Moved, _mover.TryMove(piece, 0, -1));
            Assert.AreEqual(5, piece.V);
        }

        [Test]
        public void TryMove_PastLaneEdge_BlockedBySolid()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 0, 6);
            Assert.AreEqual(MoveResult.BlockedBySolid, _mover.TryMove(piece, -1, 0));
            Assert.AreEqual(0, piece.U);
        }

        [Test]
        public void TryMove_PastCenterLine_BlockedBySolid()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, LaneFloor);
            Assert.AreEqual(MoveResult.BlockedBySolid, _mover.TryMove(piece, 0, -1));
        }

        [Test]
        public void TryMove_LockedBlockBelow_BlockedBySolid()
        {
            _grid.Set(3, 5, MockGrids.Block);
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, 6);
            Assert.AreEqual(MoveResult.BlockedBySolid, _mover.TryMove(piece, 0, -1));
        }

        [Test]
        public void TryMove_OtherFallingPieceBelow_BlockedByPiece()
        {
            _query.AddPieceCell(3, 5);
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, 6);
            Assert.AreEqual(MoveResult.BlockedByPiece, _mover.TryMove(piece, 0, -1));
        }

        [Test]
        public void DropDistance_EmptyLane_StopsAtCenter()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, 6);
            Assert.AreEqual(6 - LaneFloor, _mover.DropDistance(piece));
        }

        [Test]
        public void DropDistance_RightSidePiece_FallsLeftToCenter()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Right, 3, 6);
            Assert.AreEqual(Size - 2, _lanes.Frames.CellOf(piece, 0).X, "spawned at the right edge");

            int distance = _mover.DropDistance(piece);
            _mover.TryMove(piece, 0, -distance);
            int minX = int.MaxValue;
            for (int i = 0; i < TetrominoShapes.CellCount; i++)
            {
                minX = System.Math.Min(minX, _lanes.Frames.CellOf(piece, i).X);
            }
            Assert.AreEqual(LaneFloor, minX);
        }

        [Test]
        public void TryRotateClockwise_AgainstLaneEdge_KicksInward()
        {
            Piece piece = CreatePiece(TetrominoType.T, Side.Top, 0, 5);
            piece.SetPlacement(-1, 5, 1);
            Assert.AreEqual(CellState.Empty, _mover.CheckPlacement(piece, piece.U, piece.V, piece.Rotation));

            Assert.IsTrue(_mover.TryRotateClockwise(piece));
            Assert.AreEqual(2, piece.Rotation);
            Assert.AreEqual(0, piece.U);
        }

        [Test]
        public void TryPushOut_OverlappingBlock_MovesAgainstGravity()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, LaneFloor);
            _grid.Set(3, LaneFloor, MockGrids.Block);
            Assert.IsTrue(_mover.TryPushOut(piece));
            Assert.AreEqual(LaneFloor + 1, piece.V);
        }

        [Test]
        public void TryPushOut_LaneFull_Fails()
        {
            Piece piece = CreatePiece(TetrominoType.O, Side.Top, 3, LaneFloor);
            for (int y = LaneFloor; y < Size; y++)
            {
                _grid.Set(3, y, MockGrids.Block);
            }
            Assert.IsFalse(_mover.TryPushOut(piece));
        }

        private static Piece CreatePiece(TetrominoType type, Side side, int u, int v)
        {
            Piece piece = new Piece();
            piece.Reset(type, side, u, v);
            return piece;
        }
    }
}
