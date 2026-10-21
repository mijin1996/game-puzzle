namespace Quadrop.Game
{
    /// <summary>기본 상태: 낙하가 진행되고 모든 입력을 받는다.</summary>
    internal sealed class PlayingState : GameStateBase
    {
        public PlayingState(GameContext context) : base(context)
        {
        }

        public override GameStateId Id => GameStateId.Playing;

        public override void Tick(float deltaSeconds)
        {
            Context.AdvancePieces(deltaSeconds);
        }

        public override void Handle(GameCommand command)
        {
            switch (command)
            {
                case GameCommand.MoveLeft:
                    Context.Controller.Shift(-1);
                    break;
                case GameCommand.MoveRight:
                    Context.Controller.Shift(1);
                    break;
                case GameCommand.RotatePiece:
                    Context.Controller.RotateClockwise();
                    break;
                case GameCommand.HardDrop:
                    if (Context.Controller.HardDrop())
                    {
                        Context.CheckLines();
                    }
                    break;
                case GameCommand.Hold:
                    Context.Controller.Hold();
                    break;
                case GameCommand.RotateBoardClockwise:
                    TryRotateBoard(true);
                    break;
                case GameCommand.RotateBoardCounterClockwise:
                    TryRotateBoard(false);
                    break;
                case GameCommand.Pause:
                    Context.Machine.Change(GameStateId.Paused);
                    break;
                default:
                    break;
            }
        }

        private void TryRotateBoard(bool clockwise)
        {
            if (!Context.Control.TryRotateBoard(clockwise, Context.Spawners.Active))
            {
                return;
            }
            Context.Events.RaiseControlChanged(Context.Control.Controlled);
            Context.Machine.Change(GameStateId.Rotating);
        }
    }
}
