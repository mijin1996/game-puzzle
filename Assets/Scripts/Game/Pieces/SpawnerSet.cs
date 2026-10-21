using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>변별 스포너: 어느 변이 열렸는지와 변마다 낙하 중인 피스 하나를 관리한다.</summary>
    internal sealed class SpawnerSet
    {
        private readonly int _boardSize;
        private readonly Piece[] _pieces = new Piece[SideExtensions.Count];
        private readonly PieceTimer[] _timers = new PieceTimer[SideExtensions.Count];
        private readonly bool[] _hasPiece = new bool[SideExtensions.Count];
        private readonly TetrominoType[] _queuedTypes = new TetrominoType[SideExtensions.Count];

        public SideSet Active { get; private set; }

        public SpawnerSet(int boardSize)
        {
            if (boardSize < GridModel.MinSize)
            {
                throw new ArgumentOutOfRangeException(nameof(boardSize));
            }

            _boardSize = boardSize;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                _pieces[i] = new Piece();
                _timers[i] = new PieceTimer();
            }
        }

        public bool TryGetPiece(Side side, out Piece piece)
        {
            int index = (int)side;
            piece = _hasPiece[index] ? _pieces[index] : null;
            return _hasPiece[index];
        }

        public PieceTimer TimerOf(Side side)
        {
            return _timers[(int)side];
        }

        public void Reset(SideSet active)
        {
            Active = active;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                _hasPiece[i] = false;
                _queuedTypes[i] = TetrominoType.None;
                _timers[i].Reset();
            }
        }

        public void Activate(SideSet sides)
        {
            Active = Active.Union(sides);
        }

        public void Remove(Side side)
        {
            _hasPiece[(int)side] = false;
        }

        /// <summary>다음 생성 때 순서 대신 이 종류를 쓴다 (홀드 교체용).</summary>
        public void QueueType(Side side, TetrominoType type)
        {
            _queuedTypes[(int)side] = type;
        }

        /// <summary>활성 변 중 피스가 없는 곳에 새 피스를 놓는다.</summary>
        public SpawnOutcome FillEmpty(IPieceSequence sequence, PieceMover mover, GameEvents events)
        {
            SpawnOutcome outcome = SpawnOutcome.Ready;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                Side side = (Side)i;
                if (!Active.Contains(side) || _hasPiece[i])
                {
                    continue;
                }

                bool useQueued = _queuedTypes[i] != TetrominoType.None;
                TetrominoType type = useQueued ? _queuedTypes[i] : sequence.Peek(0);
                int boxSize = TetrominoShapes.BoxSize(type);
                int u = (_boardSize - boxSize) / 2;
                int v = _boardSize - boxSize;

                Piece piece = _pieces[i];
                piece.Reset(type, side, u, v);
                CellState state = mover.CheckPlacement(piece, u, v, 0);
                if (state == CellState.Solid)
                {
                    return SpawnOutcome.Blocked;
                }
                if (state == CellState.Piece)
                {
                    outcome = SpawnOutcome.Pending;
                    continue;
                }

                if (useQueued)
                {
                    _queuedTypes[i] = TetrominoType.None;
                }
                else
                {
                    sequence.Next();
                }

                _hasPiece[i] = true;
                _timers[i].Reset();
                events.RaisePieceSpawned(side);
            }
            return outcome;
        }
    }
}
