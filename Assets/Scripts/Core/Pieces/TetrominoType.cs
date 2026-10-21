namespace Quadrop.Core
{
    /// <summary>피스 7종. 값은 GridModel 셀에 그대로 저장된다 (0 = 빈칸).</summary>
    public enum TetrominoType : byte
    {
        None = 0,
        I = 1,
        O = 2,
        T = 3,
        S = 4,
        Z = 5,
        J = 6,
        L = 7
    }
}
