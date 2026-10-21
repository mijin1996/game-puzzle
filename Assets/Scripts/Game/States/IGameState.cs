namespace Quadrop.Game
{
    internal interface IGameState
    {
        GameStateId Id { get; }
        CommandPolicy Policy { get; }

        void Enter();

        void Tick(float deltaSeconds);

        void Handle(GameCommand command);

        void Exit();
    }
}
