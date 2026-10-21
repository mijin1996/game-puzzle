namespace Quadrop.Game
{
    internal enum SpawnOutcome
    {
        /// <summary>필요한 피스를 모두 생성했다.</summary>
        Ready,

        /// <summary>다른 낙하 피스가 생성 위치를 지나가는 중이라 다음 틱에 다시 시도한다.</summary>
        Pending,

        /// <summary>고정 블록이 생성 위치를 막았다 → 게임오버</summary>
        Blocked
    }
}
