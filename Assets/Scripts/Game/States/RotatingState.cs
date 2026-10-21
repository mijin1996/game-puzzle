namespace Quadrop.Game
{
    /// <summary>판 회전 연출 중: 낙하는 계속되고, 입력은 버리지 않고 연출이 끝날 때까지 미뤄 둔다.</summary>
    internal sealed class RotatingState : GameStateBase
    {
        private float _remaining;

        public RotatingState(GameContext context) : base(context)
        {
        }

        public override GameStateId Id => GameStateId.Rotating;

        public override CommandPolicy Policy => CommandPolicy.Defer;

        public override void Enter()
        {
            _remaining = Context.Rules.BoardRotateSeconds;
        }

        public override void Tick(float deltaSeconds)
        {
            _remaining -= deltaSeconds;
            Context.AdvancePieces(deltaSeconds);
            if (Context.Machine.CurrentId == GameStateId.Rotating && _remaining <= 0f)
            {
                Context.Machine.Change(GameStateId.Playing);
            }
        }
    }
}
