namespace Quadrop.Game
{
    /// <summary>입력 쪽(Infrastructure)이 게임에 명령을 넣는 통로. 입력 코드가 GameSession 구현을 몰라도 되게.</summary>
    public interface IGameCommandSink
    {
        void Enqueue(GameCommand command);

        void SetSoftDrop(bool isHeld);
    }
}
