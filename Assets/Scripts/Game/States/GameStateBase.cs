using System;

namespace Quadrop.Game
{
    internal abstract class GameStateBase : IGameState
    {
        protected GameContext Context { get; }

        protected GameStateBase(GameContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public abstract GameStateId Id { get; }

        public virtual CommandPolicy Policy => CommandPolicy.Process;

        public virtual void Enter()
        {
        }

        public virtual void Tick(float deltaSeconds)
        {
        }

        public virtual void Handle(GameCommand command)
        {
        }

        public virtual void Exit()
        {
        }
    }
}
