using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>새 변이 열리는 연출 중: 잠깐 멈추고 안내를 띄운 뒤 그 변에서 피스가 나오기 시작한다.</summary>
    internal sealed class UnlockingState : GameStateBase
    {
        private float _remaining;

        public UnlockingState(GameContext context) : base(context)
        {
        }

        public override GameStateId Id => GameStateId.Unlocking;

        public override CommandPolicy Policy => CommandPolicy.Discard;

        public override void Enter()
        {
            _remaining = Context.Rules.UnlockDelaySeconds;
            SideSet pending = Context.PendingUnlocks;
            Context.Spawners.Activate(pending);
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                Side side = (Side)i;
                if (pending.Contains(side))
                {
                    Context.Events.RaiseSideUnlocked(side);
                }
            }
        }

        public override void Tick(float deltaSeconds)
        {
            _remaining -= deltaSeconds;
            if (_remaining <= 0f)
            {
                Context.Machine.Change(GameStateId.Playing);
            }
        }

        public override void Exit()
        {
            Context.PendingUnlocks = SideSet.None;
        }
    }
}
