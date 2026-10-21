using Quadrop.Core;
using Quadrop.Game;

namespace Quadrop.Tests
{
    public sealed class MockHighScoreRepository : IHighScoreRepository
    {
        private int? _stored;

        public int SaveCount { get; private set; }
        public bool FailSaves { get; set; }

        public MockHighScoreRepository(int? stored)
        {
            _stored = stored;
        }

        public int? Stored => _stored;

        public Result<int> Load()
        {
            return _stored.HasValue ? Result<int>.Ok(_stored.Value) : Result<int>.Fail("empty");
        }

        public Result<bool> Save(int score)
        {
            SaveCount++;
            if (FailSaves)
            {
                return Result<bool>.Fail("disk full");
            }
            _stored = score;
            return Result<bool>.Ok(true);
        }
    }
}
