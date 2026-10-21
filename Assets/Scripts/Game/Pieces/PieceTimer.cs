namespace Quadrop.Game
{
    /// <summary>피스 하나의 낙하·고정 대기 시간 (변마다 하나씩 재사용)</summary>
    internal sealed class PieceTimer
    {
        public float Fall { get; set; }
        public float Lock { get; set; }
        public int LockResets { get; set; }

        public void Reset()
        {
            Fall = 0f;
            Lock = 0f;
            LockResets = 0;
        }
    }
}
