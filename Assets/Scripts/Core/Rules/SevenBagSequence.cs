using System;
using System.Collections.Generic;

namespace Quadrop.Core
{
    // 패턴: Strategy — 피스 생성 규칙(7-bag, 완전 랜덤 등)을 게임 흐름 코드 수정 없이 바꾸려고
    /// <summary>7종을 한 묶음으로 섞어서 하나씩 꺼낸다. 같은 피스가 너무 오래 안 나오는 일을 막는다.</summary>
    public sealed class SevenBagSequence : IPieceSequence
    {
        private readonly IRandomSource _random;
        private readonly List<TetrominoType> _queue;
        private readonly TetrominoType[] _bag;

        public SevenBagSequence(IRandomSource random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _queue = new List<TetrominoType>(TetrominoShapes.TypeCount * 2);
            _bag = new TetrominoType[TetrominoShapes.TypeCount];
        }

        public TetrominoType Next()
        {
            EnsureFilled(1);
            TetrominoType next = _queue[0];
            _queue.RemoveAt(0);
            return next;
        }

        public TetrominoType Peek(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            EnsureFilled(index + 1);
            return _queue[index];
        }

        public void Reset()
        {
            _queue.Clear();
        }

        private void EnsureFilled(int count)
        {
            while (_queue.Count < count)
            {
                AddShuffledBag();
            }
        }

        private void AddShuffledBag()
        {
            for (int i = 0; i < _bag.Length; i++)
            {
                _bag[i] = TetrominoShapes.FromIndex(i);
            }

            for (int i = _bag.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                TetrominoType temp = _bag[i];
                _bag[i] = _bag[j];
                _bag[j] = temp;
            }

            for (int i = 0; i < _bag.Length; i++)
            {
                _queue.Add(_bag[i]);
            }
        }
    }
}
