using System;

namespace Quadrop.Core
{
    /// <summary>N×N 판의 고정된 블록 상태. 데이터는 절대 회전하지 않는다.</summary>
    public sealed class GridModel : IReadOnlyGrid
    {
        public const int MinSize = 4;
        public const byte Empty = 0;

        private readonly byte[] _cells;

        public int Size { get; }

        public GridModel(int size)
        {
            if (size < MinSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Size = size;
            _cells = new byte[size * size];
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size && y < Size;
        }

        public bool IsInside(GridPos pos)
        {
            return IsInside(pos.X, pos.Y);
        }

        public byte Get(int x, int y)
        {
            return _cells[IndexOf(x, y)];
        }

        public byte Get(GridPos pos)
        {
            return Get(pos.X, pos.Y);
        }

        /// <summary>판 안이고 블록이 있으면 true. 판 밖은 false (판 밖 판정은 IsInside로 따로 한다).</summary>
        public bool IsOccupied(GridPos pos)
        {
            return IsInside(pos) && _cells[pos.Y * Size + pos.X] != Empty;
        }

        public void Set(int x, int y, byte value)
        {
            _cells[IndexOf(x, y)] = value;
        }

        public void Set(GridPos pos, byte value)
        {
            Set(pos.X, pos.Y, value);
        }

        public bool IsRowFull(int y)
        {
            for (int x = 0; x < Size; x++)
            {
                if (_cells[IndexOf(x, y)] == Empty)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsColumnFull(int x)
        {
            for (int y = 0; y < Size; y++)
            {
                if (_cells[IndexOf(x, y)] == Empty)
                {
                    return false;
                }
            }
            return true;
        }

        public void CopyRow(int fromY, int toY)
        {
            for (int x = 0; x < Size; x++)
            {
                _cells[IndexOf(x, toY)] = _cells[IndexOf(x, fromY)];
            }
        }

        public void CopyColumn(int fromX, int toX)
        {
            for (int y = 0; y < Size; y++)
            {
                _cells[IndexOf(toX, y)] = _cells[IndexOf(fromX, y)];
            }
        }

        public void ClearRow(int y)
        {
            for (int x = 0; x < Size; x++)
            {
                _cells[IndexOf(x, y)] = Empty;
            }
        }

        public void ClearColumn(int x)
        {
            for (int y = 0; y < Size; y++)
            {
                _cells[IndexOf(x, y)] = Empty;
            }
        }

        public void ClearAll()
        {
            Array.Clear(_cells, 0, _cells.Length);
        }

        private int IndexOf(int x, int y)
        {
            if (!IsInside(x, y))
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"({x}, {y}) is outside the {Size}x{Size} grid");
            }
            return y * Size + x;
        }
    }
}
