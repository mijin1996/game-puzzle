using System;

namespace Quadrop.Core
{
    /// <summary>한 변 레인의 한 줄 (Depth = 로컬 v)</summary>
    public readonly struct LaneLine : IEquatable<LaneLine>
    {
        public Side Side { get; }
        public int Depth { get; }

        public LaneLine(Side side, int depth)
        {
            Side = side;
            Depth = depth;
        }

        public bool Equals(LaneLine other)
        {
            return Side == other.Side && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            return obj is LaneLine other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((int)Side * 397) ^ Depth;
        }

        public override string ToString()
        {
            return $"{Side}:{Depth}";
        }
    }
}
