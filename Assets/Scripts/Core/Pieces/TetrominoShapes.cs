using System;

namespace Quadrop.Core
{
    /// <summary>
    /// 종류·회전별 셀 오프셋 표. 시작할 때 한 번만 만들고 이후에는 읽기만 해서 GC가 없다.
    /// 좌표는 피스 박스 안의 로컬 좌표 (x = 오른쪽, y = 위쪽, 박스 왼쪽 아래가 원점).
    /// </summary>
    public static class TetrominoShapes
    {
        public const int TypeCount = 7;
        public const int CellCount = 4;
        public const int RotationCount = 4;

        // 주의: Cells보다 먼저 선언해야 정적 초기화 순서가 맞다.
        private static readonly int[] BoxSizes = { 4, 2, 3, 3, 3, 3, 3 };

        private static readonly GridPos[][][] Cells = BuildCells();

        public static int BoxSize(TetrominoType type)
        {
            return BoxSizes[IndexOf(type)];
        }

        public static GridPos GetCell(TetrominoType type, int rotation, int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= CellCount)
            {
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            }
            return Cells[IndexOf(type)][WrapRotation(rotation)][cellIndex];
        }

        public static int WrapRotation(int rotation)
        {
            return ((rotation % RotationCount) + RotationCount) % RotationCount;
        }

        public static TetrominoType FromIndex(int index)
        {
            if (index < 0 || index >= TypeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return (TetrominoType)(index + 1);
        }

        private static int IndexOf(TetrominoType type)
        {
            int index = (int)type - 1;
            if (index < 0 || index >= TypeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "TetrominoType.None has no shape");
            }
            return index;
        }

        private static GridPos[][][] BuildCells()
        {
            GridPos[][] spawnShapes =
            {
                new[] { P(0, 2), P(1, 2), P(2, 2), P(3, 2) },   // I
                new[] { P(0, 0), P(1, 0), P(0, 1), P(1, 1) },   // O
                new[] { P(0, 1), P(1, 1), P(2, 1), P(1, 2) },   // T
                new[] { P(0, 1), P(1, 1), P(1, 2), P(2, 2) },   // S
                new[] { P(0, 2), P(1, 2), P(1, 1), P(2, 1) },   // Z
                new[] { P(0, 2), P(0, 1), P(1, 1), P(2, 1) },   // J
                new[] { P(2, 2), P(0, 1), P(1, 1), P(2, 1) }    // L
            };

            GridPos[][][] result = new GridPos[TypeCount][][];
            for (int type = 0; type < TypeCount; type++)
            {
                int boxSize = BoxSizes[type];
                result[type] = new GridPos[RotationCount][];
                result[type][0] = spawnShapes[type];
                for (int rotation = 1; rotation < RotationCount; rotation++)
                {
                    result[type][rotation] = RotateInBox(result[type][rotation - 1], boxSize);
                }
            }
            return result;
        }

        /// <summary>박스 안에서 시계 방향 90° 회전: (x, y) → (y, n - 1 - x)</summary>
        private static GridPos[] RotateInBox(GridPos[] cells, int boxSize)
        {
            GridPos[] rotated = new GridPos[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                rotated[i] = new GridPos(cells[i].Y, boxSize - 1 - cells[i].X);
            }
            return rotated;
        }

        private static GridPos P(int x, int y)
        {
            return new GridPos(x, y);
        }
    }
}
