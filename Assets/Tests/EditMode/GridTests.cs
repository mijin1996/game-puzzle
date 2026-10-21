using NUnit.Framework;
using Quadrop.Core;

namespace Quadrop.Tests
{
    public sealed class GridTests
    {
        private const int Size = 8;

        [Test]
        public void GridPos_SameCoordinates_AreEqual()
        {
            Assert.AreEqual(new GridPos(2, 3), new GridPos(2, 3));
            Assert.AreEqual(new GridPos(2, 3).GetHashCode(), new GridPos(2, 3).GetHashCode());
            Assert.AreNotEqual(new GridPos(2, 3), new GridPos(3, 2));
        }

        [Test]
        public void RotateClockwise_Down_PointsLeft()
        {
            Assert.AreEqual(new GridPos(-1, 0), new GridPos(0, -1).RotateClockwise());
        }

        [Test]
        public void IsInside_Edges_TrueAndOutsideFalse()
        {
            GridModel grid = new GridModel(Size);
            Assert.IsTrue(grid.IsInside(0, 0));
            Assert.IsTrue(grid.IsInside(Size - 1, Size - 1));
            Assert.IsFalse(grid.IsInside(-1, 0));
            Assert.IsFalse(grid.IsInside(0, Size));
        }

        [Test]
        public void IsRowFull_FilledRow_True()
        {
            GridModel grid = MockGrids.Empty(Size);
            MockGrids.FillRow(grid, 2);
            Assert.IsTrue(grid.IsRowFull(2));
            Assert.IsFalse(grid.IsRowFull(3));
        }

        [Test]
        public void Constructor_TooSmall_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GridModel(GridModel.MinSize - 1));
        }

        [TestCase(Side.Top)]
        [TestCase(Side.Right)]
        [TestCase(Side.Bottom)]
        [TestCase(Side.Left)]
        public void ToGrid_LocalCorners_StayInsideBoard(Side side)
        {
            GridModel grid = new GridModel(Size);
            SideFrame frame = new SideFrame(side, Size);
            Assert.IsTrue(grid.IsInside(frame.ToGrid(0, 0)));
            Assert.IsTrue(grid.IsInside(frame.ToGrid(Size - 1, 0)));
            Assert.IsTrue(grid.IsInside(frame.ToGrid(0, Size - 1)));
            Assert.IsTrue(grid.IsInside(frame.ToGrid(Size - 1, Size - 1)));
        }

        [TestCase(Side.Top)]
        [TestCase(Side.Right)]
        [TestCase(Side.Bottom)]
        [TestCase(Side.Left)]
        public void ToGrid_LocalTopRow_IsOnThatSidesEdge(Side side)
        {
            SideFrame frame = new SideFrame(side, Size);
            GridPos cell = frame.ToGrid(3, Size - 1);
            GridPos outward = side.Outward();
            if (outward.Y > 0)
            {
                Assert.AreEqual(Size - 1, cell.Y);
            }
            else if (outward.Y < 0)
            {
                Assert.AreEqual(0, cell.Y);
            }
            else if (outward.X > 0)
            {
                Assert.AreEqual(Size - 1, cell.X);
            }
            else
            {
                Assert.AreEqual(0, cell.X);
            }
        }

        [Test]
        public void ToGrid_TopSide_IsIdentity()
        {
            SideFrame frame = new SideFrame(Side.Top, Size);
            Assert.AreEqual(new GridPos(2, 5), frame.ToGrid(2, 5));
        }

        [Test]
        public void SideSet_WithAndWithout_TracksMembers()
        {
            SideSet set = SideSet.Of(Side.Top).With(Side.Left);
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(Side.Left));
            Assert.IsFalse(set.Contains(Side.Right));
            Assert.AreEqual(SideSet.Of(Side.Left), set.Without(SideSet.Of(Side.Top)));
            Assert.AreEqual(4, SideSet.All.Count);
            Assert.IsTrue(SideSet.None.IsEmpty);
        }

        [Test]
        public void BoardAngle_EachSide_IsQuarterTurns()
        {
            Assert.AreEqual(0, Side.Top.BoardAngleDegrees());
            Assert.AreEqual(90, Side.Right.BoardAngleDegrees());
            Assert.AreEqual(270, Side.Left.BoardAngleDegrees());
        }
    }
}
