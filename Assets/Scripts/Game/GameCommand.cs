namespace Quadrop.Game
{
    /// <summary>플레이어 입력 명령. 좌우는 항상 "화면 기준"이다.</summary>
    public enum GameCommand
    {
        MoveLeft,
        MoveRight,
        RotatePiece,
        HardDrop,
        Hold,
        RotateBoardClockwise,
        RotateBoardCounterClockwise,
        Pause,
        Restart
    }
}
