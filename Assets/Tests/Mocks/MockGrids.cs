using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>테스트용 판 만들기 (테스트마다 같은 채우기 코드를 복붙하지 않게)</summary>
    public static class MockGrids
    {
        public const byte Block = (byte)TetrominoType.T;

        public static GridModel Empty(int size)
        {
            return new GridModel(size);
        }

        public static void FillRow(GridModel grid, int y)
        {
            for (int x = 0; x < grid.Size; x++)
            {
                grid.Set(x, y, Block);
            }
        }

        public static void FillColumn(GridModel grid, int x)
        {
            for (int y = 0; y < grid.Size; y++)
            {
                grid.Set(x, y, Block);
            }
        }

        public static int CountBlocks(GridModel grid)
        {
            int count = 0;
            for (int y = 0; y < grid.Size; y++)
            {
                for (int x = 0; x < grid.Size; x++)
                {
                    if (grid.Get(x, y) != GridModel.Empty)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
