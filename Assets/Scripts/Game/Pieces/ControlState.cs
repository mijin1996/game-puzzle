using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>지금 화면 위쪽에 있는(= 조작 중인) 변</summary>
    internal sealed class ControlState
    {
        public Side Controlled { get; private set; } = Side.Top;

        public void Reset(SideSet active)
        {
            Controlled = Side.Top;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                if (active.Contains((Side)i))
                {
                    Controlled = (Side)i;
                    return;
                }
            }
        }

        /// <summary>
        /// 판을 90° 돌려 다음 활성 변을 위로 올린다.
        /// 판을 시계 방향으로 돌리면 왼쪽 변이 위로 오므로 enum 순서로는 반시계 방향이다.
        /// </summary>
        public bool TryRotateBoard(bool clockwise, SideSet active)
        {
            if (active.Count < 2)
            {
                return false;
            }

            Side candidate = Controlled;
            for (int i = 0; i < SideExtensions.Count - 1; i++)
            {
                candidate = clockwise ? candidate.CounterClockwise() : candidate.Clockwise();
                if (active.Contains(candidate))
                {
                    Controlled = candidate;
                    return true;
                }
            }
            return false;
        }
    }
}
