using System;

namespace Quadrop.Core
{
    /// <summary>
    /// 낙하 중인 피스. 위치는 자기 변의 로컬 좌표 (U = 화면 오른쪽, V = 화면 위쪽)로 저장한다.
    /// 변마다 하나씩 미리 만들어 두고 Reset으로 재사용한다.
    /// </summary>
    public sealed class Piece
    {
        public TetrominoType Type { get; private set; }
        public Side Side { get; private set; }
        public int U { get; private set; }
        public int V { get; private set; }
        public int Rotation { get; private set; }

        public void Reset(TetrominoType type, Side side, int u, int v)
        {
            if (type == TetrominoType.None)
            {
                throw new ArgumentException("A piece needs a real tetromino type", nameof(type));
            }

            Type = type;
            Side = side;
            U = u;
            V = v;
            Rotation = 0;
        }

        public void SetPlacement(int u, int v, int rotation)
        {
            U = u;
            V = v;
            Rotation = TetrominoShapes.WrapRotation(rotation);
        }

        public override string ToString()
        {
            return $"{Type}@{Side}({U}, {V}, r{Rotation})";
        }
    }
}
