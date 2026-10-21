using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>줄 지우기 연출 중: 모든 낙하가 멈추고, 끝나면 줄을 정리하고 점수·해금을 처리한다.</summary>
    internal sealed class ClearingState : GameStateBase
    {
        private float _remaining;

        public ClearingState(GameContext context) : base(context)
        {
        }

        public override GameStateId Id => GameStateId.Clearing;

        public override CommandPolicy Policy => CommandPolicy.Discard;

        public override void Enter()
        {
            _remaining = Context.Rules.ClearDelaySeconds;
        }

        public override void Tick(float deltaSeconds)
        {
            _remaining -= deltaSeconds;
            if (_remaining > 0f)
            {
                return;
            }

            SideSet active = Context.Spawners.Active;
            int lines = Context.Clearer.ClearMarkedAndCollapse(active);
            int points = Context.Score.Calculate(lines, Context.Stats.Level, active.Count);
            Context.Stats.AddClear(lines, points);
            Context.Events.RaiseBoardChanged();
            Context.Events.RaiseStatsChanged();

            if (!Context.PushOutActivePieces())
            {
                Context.Machine.Change(GameStateId.GameOver);
                return;
            }

            SideSet target = Context.Progression.ActiveSidesFor(Context.Stats.Lines);
            SideSet unlocked = target.Without(active);
            if (!unlocked.IsEmpty)
            {
                Context.PendingUnlocks = unlocked;
                Context.Machine.Change(GameStateId.Unlocking);
                return;
            }
            Context.Machine.Change(GameStateId.Playing);
        }
    }
}
