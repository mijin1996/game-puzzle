using System;

namespace Quadrop.Core
{
    /// <summary>활성화된 변들의 집합 (비트마스크라서 복사해도 GC가 없다)</summary>
    public readonly struct SideSet : IEquatable<SideSet>
    {
        private const int AllMask = (1 << SideExtensions.Count) - 1;

        private readonly int _mask;

        private SideSet(int mask)
        {
            _mask = mask & AllMask;
        }

        public static SideSet None => new SideSet(0);
        public static SideSet All => new SideSet(AllMask);

        public bool IsEmpty => _mask == 0;

        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < SideExtensions.Count; i++)
                {
                    if ((_mask & (1 << i)) != 0)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public static SideSet Of(Side side)
        {
            return new SideSet(1 << (int)side);
        }

        public bool Contains(Side side)
        {
            return (_mask & (1 << (int)side)) != 0;
        }

        public SideSet With(Side side)
        {
            return new SideSet(_mask | (1 << (int)side));
        }

        public SideSet Union(SideSet other)
        {
            return new SideSet(_mask | other._mask);
        }

        /// <summary>other에 없는 변만 남긴다.</summary>
        public SideSet Without(SideSet other)
        {
            return new SideSet(_mask & ~other._mask);
        }

        public bool Equals(SideSet other)
        {
            return _mask == other._mask;
        }

        public override bool Equals(object obj)
        {
            return obj is SideSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _mask;
        }

        public override string ToString()
        {
            return $"SideSet({Convert.ToString(_mask, 2)})";
        }
    }
}
