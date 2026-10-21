using System;
using Quadrop.Game;
using Quadrop.Infrastructure;
using UnityEngine;

namespace Quadrop.Presentation
{
    /// <summary>Unity의 Update를 게임 틱으로 넘겨주기만 한다.</summary>
    public sealed class GameLoopDriver : MonoBehaviour
    {
        private GameSession _session;
        private InputReader _input;

        public void Construct(GameSession session, InputReader input)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        private void Update()
        {
            if (_session == null)
            {
                return;
            }

            float deltaSeconds = Time.deltaTime;
            _input.Tick(deltaSeconds);
            _session.Tick(deltaSeconds);
        }
    }
}
