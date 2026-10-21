using System;
using System.Collections.Generic;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>
    /// 한 판의 진행을 바깥(입력·화면)에 보여 주는 창구.
    /// 입력은 명령 큐로 받고, 화면에는 읽기 전용 정보와 이벤트만 준다.
    /// </summary>
    public sealed class GameSession : IGameCommandSink
    {
        private const int MaxQueuedCommands = 16;
        private const float MaxDeltaSeconds = 0.1f;

        private readonly GameContext _context;
        private readonly Queue<GameCommand> _commands = new Queue<GameCommand>(MaxQueuedCommands);

        public GameEvents Events => _context.Events;
        public GameStats Stats => _context.Stats;
        public IReadOnlyGrid Grid => _context.Grid;
        public int BoardSize => _context.Rules.BoardSize;
        public LaneLayout Lanes => _context.Lanes;
        public int NextPreviewCount => _context.Rules.NextPreviewCount;
        public Side ControlledSide => _context.Control.Controlled;
        public SideSet ActiveSides => _context.Spawners.Active;
        public TetrominoType HoldType => _context.Hold.Type;
        public bool CanHold => _context.Hold.CanUse;
        public GameStateId State => _context.Machine.CurrentId;
        public bool IsStarted => _context.Machine.Current != null;
        public int NextUnlockAtLines => _context.Progression.NextUnlockAt(_context.Stats.Lines);

        /// <summary>테스트에서 판 상태를 직접 만들기 위한 통로 (InternalsVisibleTo로 테스트에만 보인다)</summary>
        internal GameContext Context => _context;

        public GameSession(GameRules rules, IProgressionRule progression, IPieceSequence sequence)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Result<GameRules> validation = rules.Validate();
            if (!validation.IsSuccess)
            {
                throw new ArgumentException(validation.Error, nameof(rules));
            }

            _context = new GameContext(rules, progression, sequence);
        }

        public void Start()
        {
            _commands.Clear();
            _context.ResetGame();
        }

        public void Enqueue(GameCommand command)
        {
            if (_commands.Count >= MaxQueuedCommands)
            {
                return;
            }
            _commands.Enqueue(command);
        }

        public void SetSoftDrop(bool isHeld)
        {
            _context.SoftDropHeld = isHeld;
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsStarted || deltaSeconds <= 0f)
            {
                return;
            }

            float clamped = MathF.Min(deltaSeconds, MaxDeltaSeconds);
            ProcessCommands();
            _context.Machine.Current.Tick(clamped);
        }

        public TetrominoType PeekNext(int index)
        {
            return _context.Sequence.Peek(index);
        }

        /// <summary>변의 낙하 피스 셀을 판 좌표로 buffer(길이 4 이상)에 채운다.</summary>
        public bool TryGetPieceCells(Side side, GridPos[] buffer, out TetrominoType type)
        {
            type = TetrominoType.None;
            if (!_context.Spawners.TryGetPiece(side, out Piece piece))
            {
                return false;
            }

            FillCells(piece, piece.V, buffer);
            type = piece.Type;
            return true;
        }

        /// <summary>조작 중인 피스가 하드 드롭하면 놓일 위치(고스트)</summary>
        public bool TryGetGhostCells(GridPos[] buffer, out TetrominoType type)
        {
            type = TetrominoType.None;
            if (!_context.Spawners.TryGetPiece(_context.Control.Controlled, out Piece piece))
            {
                return false;
            }

            int distance = _context.Mover.DropDistance(piece);
            FillCells(piece, piece.V - distance, buffer);
            type = piece.Type;
            return true;
        }

        private void FillCells(Piece piece, int v, GridPos[] buffer)
        {
            if (buffer == null || buffer.Length < TetrominoShapes.CellCount)
            {
                throw new ArgumentException("Buffer must hold at least 4 cells", nameof(buffer));
            }
            for (int i = 0; i < TetrominoShapes.CellCount; i++)
            {
                buffer[i] = _context.Frames.CellOf(piece, piece.U, v, piece.Rotation, i);
            }
        }

        private void ProcessCommands()
        {
            while (_commands.Count > 0)
            {
                IGameState state = _context.Machine.Current;
                if (state.Policy == CommandPolicy.Defer)
                {
                    return;
                }

                GameCommand command = _commands.Dequeue();
                if (state.Policy == CommandPolicy.Process)
                {
                    state.Handle(command);
                }
            }
        }
    }
}
