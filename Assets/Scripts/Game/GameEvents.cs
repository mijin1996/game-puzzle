using System;
using System.Collections.Generic;
using Quadrop.Core;

namespace Quadrop.Game
{
    // 패턴: Observer — 게임 흐름(Game)이 화면·사운드(Presentation)를 모르게 하려고
    /// <summary>화면 쪽에 알리는 이벤트 모음. 구독한 쪽은 OnDestroy에서 반드시 해제한다.</summary>
    public sealed class GameEvents
    {
        public event Action BoardChanged;
        public event Action<IReadOnlyList<GridPos>> LinesMarked;
        public event Action<Side> PieceSpawned;
        public event Action<Side> PieceLocked;
        public event Action<Side> SideUnlocked;
        public event Action<Side> ControlChanged;
        public event Action StatsChanged;
        public event Action HoldChanged;
        public event Action<GameStateId> StateChanged;

        internal void RaiseBoardChanged()
        {
            BoardChanged?.Invoke();
        }

        /// <summary>지워질 줄의 칸들 (화면에서 깜빡이기용)</summary>
        internal void RaiseLinesMarked(IReadOnlyList<GridPos> cells)
        {
            LinesMarked?.Invoke(cells);
        }

        internal void RaisePieceSpawned(Side side)
        {
            PieceSpawned?.Invoke(side);
        }

        internal void RaisePieceLocked(Side side)
        {
            PieceLocked?.Invoke(side);
        }

        internal void RaiseSideUnlocked(Side side)
        {
            SideUnlocked?.Invoke(side);
        }

        internal void RaiseControlChanged(Side side)
        {
            ControlChanged?.Invoke(side);
        }

        internal void RaiseStatsChanged()
        {
            StatsChanged?.Invoke();
        }

        internal void RaiseHoldChanged()
        {
            HoldChanged?.Invoke();
        }

        internal void RaiseStateChanged(GameStateId state)
        {
            StateChanged?.Invoke(state);
        }
    }
}
