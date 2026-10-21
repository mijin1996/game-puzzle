using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>모든 활성 피스를 같은 박자로 중력 방향으로 떨어뜨리고, 바닥에 닿은 채 시간이 지나면 고정한다.</summary>
    internal sealed class GravityStepper
    {
        private readonly PieceMover _mover;
        private readonly SpawnerSet _spawners;
        private readonly PieceLocker _locker;
        private readonly GravitySchedule _schedule;
        private readonly GameStats _stats;
        private readonly GameRules _rules;

        public GravityStepper(PieceMover mover, SpawnerSet spawners, PieceLocker locker,
            GravitySchedule schedule, GameStats stats, GameRules rules)
        {
            _mover = mover ?? throw new ArgumentNullException(nameof(mover));
            _spawners = spawners ?? throw new ArgumentNullException(nameof(spawners));
            _locker = locker ?? throw new ArgumentNullException(nameof(locker));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>이번 틱에 고정된 피스 수를 돌려준다. 처리 순서는 변 enum 순서로 고정한다.</summary>
        public int Step(float deltaSeconds, bool softDropHeld, Side controlled)
        {
            int lockedCount = 0;
            float gravitySeconds = _schedule.FallSeconds(_stats.Level);

            for (int i = 0; i < SideExtensions.Count; i++)
            {
                Side side = (Side)i;
                if (!_spawners.TryGetPiece(side, out Piece piece))
                {
                    continue;
                }

                PieceTimer timer = _spawners.TimerOf(side);
                float interval = (softDropHeld && side == controlled)
                    ? MathF.Min(gravitySeconds, _rules.SoftDropSeconds)
                    : gravitySeconds;

                timer.Fall += deltaSeconds;
                while (timer.Fall >= interval)
                {
                    timer.Fall -= interval;
                    if (_mover.TryMove(piece, 0, -1) == MoveResult.Moved)
                    {
                        timer.Lock = 0f;
                    }
                    else
                    {
                        timer.Fall = 0f;
                        break;
                    }
                }

                if (_mover.IsResting(piece))
                {
                    timer.Lock += deltaSeconds;
                    if (timer.Lock >= _rules.LockDelaySeconds)
                    {
                        _locker.Lock(side);
                        lockedCount++;
                    }
                }
                else
                {
                    timer.Lock = 0f;
                }
            }
            return lockedCount;
        }
    }
}
