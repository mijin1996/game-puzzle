namespace Quadrop.Game
{
    internal sealed class PausedState : GameStateBase
    {
        public PausedState(GameContext context) : base(context)
        {
        }

        public override GameStateId Id => GameStateId.Paused;

        public override void Handle(GameCommand command)
        {
            if (command == GameCommand.Pause)
            {
                Context.Machine.Change(GameStateId.Playing);
            }
            else if (command == GameCommand.Restart)
            {
                Context.ResetGame();
            }
        }
    }
}
