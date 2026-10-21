using System;
using System.Collections.Generic;

namespace Quadrop.Core
{
    /// <summary>
    /// 활성 변 레인에서 꽉 찬 줄을 찾아 지우고, 바깥쪽 블록을 중앙 쪽으로 한 칸씩 당겨 메운다.
    /// 규칙은 docs/architecture.md 1.6 참고.
    /// </summary>
    public sealed class LineClearer
    {
        private readonly GridModel _grid;
        private readonly LaneLayout _lanes;
        private readonly List<LaneLine> _lines;
        private readonly List<GridPos> _cells;

        public IReadOnlyList<LaneLine> MarkedLines => _lines;
        public IReadOnlyList<GridPos> MarkedCells => _cells;
        public int MarkedCount => _lines.Count;

        public LineClearer(GridModel grid, LaneLayout lanes)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
            if (grid.Size != lanes.BoardSize)
            {
                throw new ArgumentException("Grid and lane layout sizes differ");
            }

            int maxLines = SideExtensions.Count * lanes.Depth;
            _lines = new List<LaneLine>(maxLines);
            _cells = new List<GridPos>(maxLines * lanes.Width);
        }

        /// <summary>활성 변 레인의 꽉 찬 줄을 표시해 둔다 (지우지는 않음). 변마다 깊이 오름차순.</summary>
        public int MarkFullLines(SideSet activeSides)
        {
            _lines.Clear();
            _cells.Clear();
            for (int s = 0; s < SideExtensions.Count; s++)
            {
                Side side = (Side)s;
                if (!activeSides.Contains(side))
                {
                    continue;
                }
                for (int v = _lanes.MinV; v < _lanes.BoardSize; v++)
                {
                    if (IsLineFull(side, v))
                    {
                        _lines.Add(new LaneLine(side, v));
                        for (int u = _lanes.MinU; u < _lanes.MaxU; u++)
                        {
                            _cells.Add(_lanes.ToGrid(side, u, v));
                        }
                    }
                }
            }
            return MarkedCount;
        }

        /// <summary>
        /// 표시된 줄을 동시에 지우고 메운다. 메운 뒤 새로 꽉 찬 줄이 생기면 연쇄로 계속 지운다.
        /// 지운 줄의 총 개수를 돌려준다.
        /// </summary>
        public int ClearMarkedAndCollapse(SideSet activeSides)
        {
            if (MarkedCount == 0)
            {
                MarkFullLines(activeSides);
            }

            int total = 0;
            int guard = _lanes.Depth * SideExtensions.Count;
            while (MarkedCount > 0 && guard > 0)
            {
                total += MarkedCount;
                for (int i = 0; i < _cells.Count; i++)
                {
                    _grid.Set(_cells[i], GridModel.Empty);
                }

                // 같은 레인은 바깥쪽(깊이 큰) 줄부터 메워야 아직 처리하지 않은 줄 번호가 밀리지 않는다.
                for (int i = _lines.Count - 1; i >= 0; i--)
                {
                    PullTowardCenter(_lines[i].Side, _lines[i].Depth);
                }

                MarkFullLines(activeSides);
                guard--;
            }

            _lines.Clear();
            _cells.Clear();
            return total;
        }

        private bool IsLineFull(Side side, int v)
        {
            for (int u = _lanes.MinU; u < _lanes.MaxU; u++)
            {
                if (!_grid.IsOccupied(_lanes.ToGrid(side, u, v)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>깊이 v보다 바깥쪽 줄들을 한 칸씩 중앙 쪽으로 당기고, 가장 바깥 줄을 비운다.</summary>
        private void PullTowardCenter(Side side, int v)
        {
            int outermost = _lanes.BoardSize - 1;
            for (int depth = v; depth < outermost; depth++)
            {
                for (int u = _lanes.MinU; u < _lanes.MaxU; u++)
                {
                    byte outer = _grid.Get(_lanes.ToGrid(side, u, depth + 1));
                    _grid.Set(_lanes.ToGrid(side, u, depth), outer);
                }
            }
            for (int u = _lanes.MinU; u < _lanes.MaxU; u++)
            {
                _grid.Set(_lanes.ToGrid(side, u, outermost), GridModel.Empty);
            }
        }
    }
}
