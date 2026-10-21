namespace Quadrop.Core
{
    /// <summary>ALL SIDES 모드: 시작부터 4방향이 모두 열려 있다.</summary>
    public sealed class AllSidesProgression : IProgressionRule
    {
        public SideSet ActiveSidesFor(int totalLines)
        {
            return SideSet.All;
        }

        public int NextUnlockAt(int totalLines)
        {
            return -1;
        }
    }
}
