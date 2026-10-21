using Quadrop.Core;
using Quadrop.Game;
using UnityEngine;

namespace Quadrop.Infrastructure
{
    /// <summary>최고 점수를 PlayerPrefs에 저장한다 (웹 빌드에서도 동작).</summary>
    public sealed class PlayerPrefsHighScoreRepository : IHighScoreRepository
    {
        private const string Key = "quadrop.highscore";

        public Result<int> Load()
        {
            if (!PlayerPrefs.HasKey(Key))
            {
                return Result<int>.Fail("No saved high score yet");
            }

            int value = PlayerPrefs.GetInt(Key, 0);
            if (value < 0)
            {
                return Result<int>.Fail($"Saved high score is invalid ({value})");
            }
            return Result<int>.Ok(value);
        }

        public Result<bool> Save(int score)
        {
            if (score < 0)
            {
                return Result<bool>.Fail("Score cannot be negative");
            }

            try
            {
                PlayerPrefs.SetInt(Key, score);
                PlayerPrefs.Save();
                return Result<bool>.Ok(true);
            }
            catch (PlayerPrefsException exception)
            {
                return Result<bool>.Fail(exception.Message);
            }
        }
    }
}
