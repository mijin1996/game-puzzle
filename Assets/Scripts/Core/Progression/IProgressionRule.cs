namespace Quadrop.Core
{
    // 패턴: Strategy — 모드마다 방향 해금 규칙만 바꿔 끼우려고 (새 모드 = 새 구현체 추가)
    public interface IProgressionRule
    {
        /// <summary>누적 지운 줄 수에서 활성화돼 있어야 할 변들</summary>
        SideSet ActiveSidesFor(int totalLines);

        /// <summary>다음 변이 열리는 누적 줄 수. 더 열릴 변이 없으면 -1.</summary>
        int NextUnlockAt(int totalLines);
    }
}
