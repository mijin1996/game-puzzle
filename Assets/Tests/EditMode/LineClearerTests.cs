using NUnit.Framework;
using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>판 12, 레인 폭 4: 위 레인 = x 4~7, y 6~11 / 왼쪽 레인 = x 0~5, y 4~7</summary>
    public sealed class LineClearerTests
    {
        private const int Size = 12;
        private const int Width = 4;
        private const int LaneLeft = 4;
        private const int Center = Size / 2;

        private GridModel _grid;
        private LaneLayout _lanes;
        private LineClearer _clearer;

        [SetUp]
        public void SetUp()
        {
            _grid = MockGrids.Empty(Size);
            _lanes = new LaneLayout(new BoardFrames(Size), Width);
            _clearer = new LineClearer(_grid, _lanes);
        }

        [Test]
        public void MarkFullLines_NothingFull_ReturnsZero()
        {
            Assert.AreEqual(0, _clearer.MarkFullLines(SideSet.All));
        }

        [Test]
        public void MarkFullLines_FullRowInTopLane_MarksLaneCellsOnly()
        {
            FillTopLaneRow(Center);
            Assert.AreEqual(1, _clearer.MarkFullLines(SideSet.Of(Side.Top)));
            Assert.AreEqual(Width, _clearer.MarkedCells.Count);
        }

        [Test]
        public void MarkFullLines_InactiveSide_Ignored()
        {
            FillTopLaneRow(Center);
            Assert.AreEqual(0, _clearer.MarkFullLines(SideSet.Of(Side.Left)));
        }

        [Test]
        public void Clear_TopLane_OuterBlocksMoveTowardCenter()
        {
            FillTopLaneRow(Center);
            _grid.Set(LaneLeft + 1, Center + 1, MockGrids.Block);

            _clearer.MarkFullLines(SideSet.Of(Side.Top));
            Assert.AreEqual(1, _clearer.ClearMarkedAndCollapse(SideSet.Of(Side.Top)));
            Assert.AreNotEqual(GridModel.Empty, _grid.Get(LaneLeft + 1, Center));
            Assert.AreEqual(GridModel.Empty, _grid.Get(LaneLeft + 1, Center + 1));
            Assert.AreEqual(1, MockGrids.CountBlocks(_grid));
        }

        [Test]
        public void Clear_TwoRows_BlockMovesTwo()
        {
            FillTopLaneRow(Center);
            FillTopLaneRow(Center + 1);
            _grid.Set(LaneLeft + 2, Center + 2, MockGrids.Block);

            Assert.AreEqual(2, _clearer.ClearMarkedAndCollapse(SideSet.Of(Side.Top)));
            Assert.AreNotEqual(GridModel.Empty, _grid.Get(LaneLeft + 2, Center));
            Assert.AreEqual(1, MockGrids.CountBlocks(_grid));
        }

        [Test]
        public void Clear_LeftLaneColumn_BlocksMoveRightTowardCenter()
        {
            // 왼쪽 레인의 가장 안쪽 줄 = x 5, y 4~7
            for (int y = LaneLeft; y < LaneLeft + Width; y++)
            {
                _grid.Set(Center - 1, y, MockGrids.Block);
            }
            _grid.Set(Center - 2, LaneLeft, MockGrids.Block);

            Assert.AreEqual(1, _clearer.ClearMarkedAndCollapse(SideSet.Of(Side.Left)));
            Assert.AreNotEqual(GridModel.Empty, _grid.Get(Center - 1, LaneLeft));
            Assert.AreEqual(1, MockGrids.CountBlocks(_grid));
        }

        [Test]
        public void Layout_LocalRoundTrip_MatchesForEverySide()
        {
            for (int s = 0; s < SideExtensions.Count; s++)
            {
                Side side = (Side)s;
                GridPos cell = _lanes.ToGrid(side, 5, 9);
                _lanes.ToLocal(side, cell, out int u, out int v);
                Assert.AreEqual(5, u, side.ToString());
                Assert.AreEqual(9, v, side.ToString());
            }
        }

        [Test]
        public void Layout_CornerCell_IsOutsideEveryLane()
        {
            Assert.IsFalse(_lanes.IsInsideAnyLane(new GridPos(0, 0)));
            Assert.IsTrue(_lanes.IsInsideAnyLane(new GridPos(Center, Center)));
        }

        private void FillTopLaneRow(int y)
        {
            for (int x = LaneLeft; x < LaneLeft + Width; x++)
            {
                _grid.Set(x, y, MockGrids.Block);
            }
        }
    }
}
