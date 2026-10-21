using System;

namespace Quadrop.Core
{
    /// <summary>판 위의 칸 좌표 (원점은 왼쪽 아래, X는 오른쪽, Y는 위쪽)</summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public int X { get; }
        public int Y { get; }

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>시계 방향으로 90° 돌린 벡터: (x, y) → (y, -x)</summary>
        public GridPos RotateClockwise()
        {
            return new GridPos(Y, -X);
        }

        public static GridPos operator +(GridPos a, GridPos b)
        {
            return new GridPos(a.X + b.X, a.Y + b.Y);
        }

        public static GridPos operator -(GridPos a, GridPos b)
        {
            return new GridPos(a.X - b.X, a.Y - b.Y);
        }

        public static GridPos operator *(GridPos a, int scale)
        {
            return new GridPos(a.X * scale, a.Y * scale);
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return !a.Equals(b);
        }

        public bool Equals(GridPos other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
