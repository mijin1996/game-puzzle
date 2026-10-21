using Quadrop.Core;

namespace Quadrop.Tests
{
    /// <summary>
    /// 작은 판(12, 레인 폭 4, 레인 깊이 6)과 짧은 연출 시간을 쓰는 테스트용 규칙.
    /// 위쪽 레인은 로컬 u 4~7, v 6~11. O 피스는 u 5, v 10에서 생성된다.
    /// </summary>
    public static class TestRules
    {
        public const float ShortDelay = 0.05f;
        public const int BoardSize = 12;
        public const int LaneWidth = 4;

        public static GameRules Create(int[] unlockThresholds = null)
        {
            GameRules rules = new GameRules();
            rules.BoardSize = BoardSize;
            rules.LaneWidth = LaneWidth;
            rules.ClearDelaySeconds = ShortDelay;
            rules.UnlockDelaySeconds = ShortDelay;
            rules.BoardRotateSeconds = ShortDelay;
            if (unlockThresholds != null)
            {
                rules.UnlockThresholds = unlockThresholds;
            }
            return rules;
        }
    }
}
