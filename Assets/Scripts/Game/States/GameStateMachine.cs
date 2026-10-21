using System;

namespace Quadrop.Game
{
    // 패턴: State — 상태마다 낙하·입력 허용 여부가 달라서, 큰 if/switch 대신 상태 객체로 나누려고
    internal sealed class GameStateMachine
    {
        private const int StateCount = 6;

        private readonly IGameState[] _states = new IGameState[StateCount];
        private readonly GameEvents _events;

        public IGameState Current { get; private set; }

        public GameStateId CurrentId => Current != null ? Current.Id : GameStateId.Playing;

        public GameStateMachine(GameEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Register(IGameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            _states[(int)state.Id] = state;
        }

        public void Change(GameStateId id)
        {
            IGameState next = _states[(int)id];
            if (next == null)
            {
                throw new InvalidOperationException($"State {id} is not registered");
            }

            Current?.Exit();
            Current = next;
            Current.Enter();
            _events.RaiseStateChanged(id);
        }
    }
}
