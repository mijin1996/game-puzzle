namespace Quadrop.Core
{
    /// <summary>랜덤 추상화 — 테스트에서 고정된 값을 주입하려고</summary>
    public interface IRandomSource
    {
        /// <summary>0 이상 maxExclusive 미만의 정수를 돌려준다.</summary>
        int Next(int maxExclusive);
    }
}
