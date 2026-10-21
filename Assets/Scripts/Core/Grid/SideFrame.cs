namespace Quadrop.Core
{
    /// <summary>
    /// 한 변 기준의 로컬 좌표계 (u = 화면 오른쪽, v = 화면 위쪽).
    /// 피스는 자기 변의 로컬 좌표로만 움직이고, 판 좌표 변환은 이 구조체가 맡는다.
    /// 그래서 낙하는 4방향 모두 "v - 1" 하나로 처리된다.
    /// </summary>
    public readonly struct SideFrame
    {
        public GridPos Right { get; }
        public GridPos Up { get; }
        public GridPos Origin { get; }

        public SideFrame(Side side, int boardSize)
        {
            GridPos up = side.Outward();
            GridPos right = up.RotateClockwise();
            int max = boardSize - 1;
            int originX = (right.X < 0 || up.X < 0) ? max : 0;
            int originY = (right.Y < 0 || up.Y < 0) ? max : 0;

            Up = up;
            Right = right;
            Origin = new GridPos(originX, originY);
        }

        public GridPos ToGrid(int u, int v)
        {
            return new GridPos(
                Origin.X + Right.X * u + Up.X * v,
                Origin.Y + Right.Y * u + Up.Y * v);
        }
    }
}
