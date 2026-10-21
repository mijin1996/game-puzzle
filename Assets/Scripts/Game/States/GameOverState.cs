namespace Quadrop.Game
{
    internal sealed class GameOverState : GameStateBase
    {
        public GameOverState(GameContext context) : base(context)
        {
        }

        public override GameStateId Id => GameStateId.GameOver;

        public override void Enter()
        {
            Context.SoftDropHeld = false;
        }

        public override void Handle(GameCommand command)
        {
            if (command == GameCommand.Restart)
            {
                Context.ResetGame();
            }
        }
    }
}
