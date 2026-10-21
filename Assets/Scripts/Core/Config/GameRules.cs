namespace Quadrop.Core
{
    /// <summary>
    /// 게임 규칙 값 묶음 (순수 C#). Presentation의 GameConfigAsset(ScriptableObject)에서 만들어진다.
    /// 쓰기 전에 Validate()로 검사한다.
    /// </summary>
    public sealed class GameRules
    {
        public int BoardSize { get; set; } = 20;
        public int LaneWidth { get; set; } = 10;
        public float BaseFallSeconds { get; set; } = 0.8f;
        public float FallSpeedFactor { get; set; } = 0.85f;
        public float MinFallSeconds { get; set; } = 0.05f;
        public float SoftDropSeconds { get; set; } = 0.03f;
        public float LockDelaySeconds { get; set; } = 0.5f;
        public int MaxLockResets { get; set; } = 15;
        public float ClearDelaySeconds { get; set; } = 0.3f;
        public float UnlockDelaySeconds { get; set; } = 1.2f;
        public float BoardRotateSeconds { get; set; } = 0.15f;
        public int LinesPerLevel { get; set; } = 10;
        public int NextPreviewCount { get; set; } = 3;
        public int[] UnlockThresholds { get; set; } = new[] { 0, 10, 25, 45 };
        public Side[] UnlockOrder { get; set; } = new[] { Side.Top, Side.Left, Side.Bottom, Side.Right };
        public int[] LineScores { get; set; } = new[] { 100, 300, 500, 800 };
        public float[] SideMultipliers { get; set; } = new[] { 1f, 1.5f, 2f, 3f };

        public const int MinBoardSize = 12;
        public const int MaxBoardSize = 40;
        public const int MinLaneWidth = 4;
        public const int MaxPieceBoxSize = 4;
        public const int MaxPreviewCount = 5;

        public Result<GameRules> Validate()
        {
            if (BoardSize < MinBoardSize || BoardSize > MaxBoardSize)
            {
                return Result<GameRules>.Fail($"BoardSize must be {MinBoardSize}~{MaxBoardSize} (was {BoardSize})");
            }
            if (BoardSize % 2 != 0 || (BoardSize - LaneWidth) % 2 != 0)
            {
                return Result<GameRules>.Fail("BoardSize and LaneWidth must both be even so lanes stay centered");
            }
            // 레인 양옆 여백이 피스 박스보다 넓어야 생성 위치가 다른 레인과 겹치지 않는다.
            if (LaneWidth < MinLaneWidth || (BoardSize - LaneWidth) / 2 < MaxPieceBoxSize)
            {
                return Result<GameRules>.Fail($"LaneWidth must be {MinLaneWidth}~{BoardSize - MaxPieceBoxSize * 2} for BoardSize {BoardSize}");
            }
            if (BaseFallSeconds <= 0f || MinFallSeconds <= 0f || SoftDropSeconds <= 0f)
            {
                return Result<GameRules>.Fail("Fall intervals must be positive");
            }
            if (FallSpeedFactor <= 0f || FallSpeedFactor > 1f)
            {
                return Result<GameRules>.Fail("FallSpeedFactor must be in (0, 1]");
            }
            if (LockDelaySeconds < 0f || ClearDelaySeconds < 0f || UnlockDelaySeconds < 0f || BoardRotateSeconds < 0f)
            {
                return Result<GameRules>.Fail("Delays cannot be negative");
            }
            if (MaxLockResets < 0 || LinesPerLevel <= 0)
            {
                return Result<GameRules>.Fail("MaxLockResets must be >= 0 and LinesPerLevel > 0");
            }
            if (NextPreviewCount < 0 || NextPreviewCount > MaxPreviewCount)
            {
                return Result<GameRules>.Fail($"NextPreviewCount must be 0~{MaxPreviewCount}");
            }
            if (UnlockThresholds == null || UnlockOrder == null
                || UnlockThresholds.Length == 0 || UnlockThresholds.Length != UnlockOrder.Length)
            {
                return Result<GameRules>.Fail("UnlockThresholds and UnlockOrder must have the same non-zero length");
            }
            if (LineScores == null || LineScores.Length == 0 || SideMultipliers == null || SideMultipliers.Length == 0)
            {
                return Result<GameRules>.Fail("LineScores and SideMultipliers cannot be empty");
            }
            if (UnlockThresholds[0] != 0)
            {
                return Result<GameRules>.Fail("The first unlock threshold must be 0");
            }
            SideSet seen = SideSet.None;
            for (int i = 0; i < UnlockOrder.Length; i++)
            {
                if (i > 0 && UnlockThresholds[i] < UnlockThresholds[i - 1])
                {
                    return Result<GameRules>.Fail("UnlockThresholds must be ascending");
                }
                if (seen.Contains(UnlockOrder[i]))
                {
                    return Result<GameRules>.Fail($"UnlockOrder repeats {UnlockOrder[i]}");
                }
                seen = seen.With(UnlockOrder[i]);
            }
            return Result<GameRules>.Ok(this);
        }
    }
}
