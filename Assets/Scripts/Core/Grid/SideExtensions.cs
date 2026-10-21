using System;

namespace Quadrop.Core
{
    /// <summary>변(Side) 계산 모음 — 4방향을 if 분기 대신 같은 계산으로 처리하려고</summary>
    public static class SideExtensions
    {
        public const int Count = 4;
        public const int QuarterTurnDegrees = 90;

        /// <summary>판 가장자리에서 바깥을 향하는 벡터. 이 변 피스의 "화면 위쪽"이기도 하다.</summary>
        public static GridPos Outward(this Side side)
        {
            switch (side)
            {
                case Side.Top:
                    return new GridPos(0, 1);
                case Side.Right:
                    return new GridPos(1, 0);
                case Side.Bottom:
                    return new GridPos(0, -1);
                case Side.Left:
                    return new GridPos(-1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side));
            }
        }

        /// <summary>이 변의 피스가 떨어지는 방향 (바깥의 반대)</summary>
        public static GridPos Gravity(this Side side)
        {
            return side.Outward() * -1;
        }

        /// <summary>enum 순서상 다음 변 (위 → 오른쪽 → 아래 → 왼쪽)</summary>
        public static Side Clockwise(this Side side)
        {
            return (Side)(((int)side + 1) % Count);
        }

        public static Side CounterClockwise(this Side side)
        {
            return (Side)(((int)side + Count - 1) % Count);
        }

        /// <summary>이 변이 화면 위쪽에 오도록 판을 돌린 각도 (Z축, 반시계가 +)</summary>
        public static int BoardAngleDegrees(this Side side)
        {
            return (int)side * QuarterTurnDegrees;
        }
    }
}
