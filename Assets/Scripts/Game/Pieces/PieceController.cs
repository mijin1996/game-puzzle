using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>플레이어 명령을 조작 중인 변의 피스에 적용한다.</summary>
    internal sealed class PieceController
    {
        private readonly PieceMover _mover;
        private readonly SpawnerSet _spawners;
        private readonly ControlState _control;
        private readonly HoldSlot _hold;
        private readonly PieceLocker _locker;
        private readonly GameEvents _events;
        private readonly GameRules _rules;

        public PieceController(PieceMover mover, SpawnerSet spawners, ControlState control, HoldSlot hold,
            PieceLocker locker, GameEvents events, GameRules rules)
        {
            _mover = mover ?? throw new ArgumentNullException(nameof(mover));
            _spawners = spawners ?? throw new ArgumentNullException(nameof(spawners));
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _hold = hold ?? throw new ArgumentNullException(nameof(hold));
            _locker = locker ?? throw new ArgumentNullException(nameof(locker));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>화면 기준 좌우 이동 (-1 = 왼쪽, +1 = 오른쪽)</summary>
        public bool Shift(int direction)
        {
            if (!TryGetControlled(out Side side, out Piece piece))
            {
                return false;
            }
            if (_mover.TryMove(piece, Math.Sign(direction), 0) != MoveResult.Moved)
            {
                return false;
            }
            ResetLockDelay(side, piece);
            return true;
        }

        public bool RotateClockwise()
        {
            if (!TryGetControlled(out Side side, out Piece piece))
            {
                return false;
            }
            if (!_mover.TryRotateClockwise(piece))
            {
                return false;
            }
            ResetLockDelay(side, piece);
            return true;
        }

        /// <summary>막힐 때까지 떨어뜨리고 바로 고정한다. 고정했으면 true.</summary>
        public bool HardDrop()
        {
            if (!TryGetControlled(out Side side, out Piece piece))
            {
                return false;
            }

            int distance = _mover.DropDistance(piece);
            piece.SetPlacement(piece.U, piece.V - distance, piece.Rotation);
            _locker.Lock(side);
            return true;
        }

        public bool Hold()
        {
            if (!TryGetControlled(out Side side, out Piece piece))
            {
                return false;
            }
            if (!_hold.TryHold(piece.Type, out TetrominoType released))
            {
                return false;
            }

            _spawners.Remove(side);
            if (released != TetrominoType.None)
            {
                _spawners.QueueType(side, released);
            }
            _events.RaiseHoldChanged();
            return true;
        }

        private bool TryGetControlled(out Side side, out Piece piece)
        {
            side = _control.Controlled;
            return _spawners.TryGetPiece(side, out piece);
        }

        /// <summary>바닥에 닿은 상태에서 움직이면 고정 대기를 다시 시작한다 (무한 지연 방지용 횟수 제한).</summary>
        private void ResetLockDelay(Side side, Piece piece)
        {
            PieceTimer timer = _spawners.TimerOf(side);
            if (timer.LockResets >= _rules.MaxLockResets || !_mover.IsResting(piece))
            {
                return;
            }
            timer.Lock = 0f;
            timer.LockResets++;
        }
    }
}
