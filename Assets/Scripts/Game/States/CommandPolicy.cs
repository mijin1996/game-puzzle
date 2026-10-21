namespace Quadrop.Game
{
    /// <summary>상태가 쌓인 입력 명령을 어떻게 다루는지</summary>
    internal enum CommandPolicy
    {
        /// <summary>바로 처리한다.</summary>
        Process,

        /// <summary>버리지 않고 큐에 남겨 두었다가 다음 상태에서 처리한다 (판 회전 연출 중).</summary>
        Defer,

        /// <summary>버린다 (줄 지우기·해금 연출 중).</summary>
        Discard
    }
}
