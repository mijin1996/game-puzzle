using System;

namespace Quadrop.Core
{
    /// <summary>
    /// 십자(+) 모양 판의 레인 배치. 변마다 폭 W, 깊이 N/2인 레인이 있고 피스는 판 중앙 쪽으로 떨어진다.
    /// 로컬 좌표로 보면 모든 변의 레인이 같은 직사각형이다: u ∈ [MinU, MaxU), v ∈ [MinV, BoardSize).
    /// 이웃한 레인은 중앙 근처에서 겹치고, 그 겹친 칸에서 변끼리 서로 영향을 준다.
    /// </summary>
    public sealed class LaneLayout
    {
        public BoardFrames Frames { get; }
        public int BoardSize { get; }
        public int Width { get; }
        public int MinU { get; }
        public int MaxU { get; }
        public int MinV { get; }
        public int Depth => BoardSize - MinV;

        public LaneLayout(BoardFrames frames, int laneWidth)
        {
            Frames = frames ?? throw new ArgumentNullException(nameof(frames));
            if (laneWidth < GridModel.MinSize || laneWidth > frames.BoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(laneWidth));
            }

            BoardSize = frames.BoardSize;
            Width = laneWidth;
            MinU = (BoardSize - laneWidth) / 2;
            MaxU = MinU + laneWidth;
            MinV = BoardSize / 2;
        }

        public bool IsInsideLane(int u, int v)
        {
            return u >= MinU && u < MaxU && v >= MinV && v < BoardSize;
        }

        public bool IsInsideLane(Side side, GridPos cell)
        {
            ToLocal(side, cell, out int u, out int v);
            return IsInsideLane(u, v);
        }

        public GridPos ToGrid(Side side, int u, int v)
        {
            return Frames.FrameOf(side).ToGrid(u, v);
        }

        /// <summary>판 좌표 → 변 로컬 좌표 (축이 단위 벡터라 내적으로 바로 구한다)</summary>
        public void ToLocal(Side side, GridPos cell, out int u, out int v)
        {
            SideFrame frame = Frames.FrameOf(side);
            GridPos delta = cell - frame.Origin;
            u = delta.X * frame.Right.X + delta.Y * frame.Right.Y;
            v = delta.X * frame.Up.X + delta.Y * frame.Up.Y;
        }

        /// <summary>어느 변의 레인에든 들어 있는 칸인지 (십자 모양 바깥의 모서리 칸은 false)</summary>
        public bool IsInsideAnyLane(GridPos cell)
        {
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                if (IsInsideLane((Side)i, cell))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
