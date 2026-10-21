using System;

namespace Quadrop.Core
{
    /// <summary>충돌을 검사한 뒤 피스를 이동·회전시킨다. 모든 계산은 피스 변의 로컬 좌표 기준이고, 피스는 자기 레인 밖으로 못 나간다.</summary>
    public sealed class PieceMover
    {
        // 회전이 막히면 순서대로 시도할 로컬 오프셋 (u, v). v + 1은 중력 반대쪽으로 들어 올리는 것.
        private static readonly GridPos[] RotationKicks =
        {
            new GridPos(0, 0),
            new GridPos(-1, 0),
            new GridPos(1, 0),
            new GridPos(0, 1),
            new GridPos(-2, 0),
            new GridPos(2, 0)
        };

        private readonly ICellQuery _query;
        private readonly LaneLayout _lanes;

        public PieceMover(ICellQuery query, LaneLayout lanes)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
        }

        /// <summary>피스를 해당 위치에 놓았을 때 가장 강하게 막는 칸 상태. 자기 레인 밖은 Solid.</summary>
        public CellState CheckPlacement(Piece piece, int u, int v, int rotation)
        {
            CellState worst = CellState.Empty;
            for (int i = 0; i < TetrominoShapes.CellCount; i++)
            {
                GridPos local = TetrominoShapes.GetCell(piece.Type, rotation, i);
                int cellU = u + local.X;
                int cellV = v + local.Y;
                if (!_lanes.IsInsideLane(cellU, cellV))
                {
                    return CellState.Solid;
                }

                GridPos cell = _lanes.ToGrid(piece.Side, cellU, cellV);
                CellState state = _query.Query(cell, piece);
                if (state == CellState.Solid)
                {
                    return CellState.Solid;
                }
                if (state > worst)
                {
                    worst = state;
                }
            }
            return worst;
        }

        public MoveResult TryMove(Piece piece, int deltaU, int deltaV)
        {
            int targetU = piece.U + deltaU;
            int targetV = piece.V + deltaV;
            CellState state = CheckPlacement(piece, targetU, targetV, piece.Rotation);
            if (state == CellState.Empty)
            {
                piece.SetPlacement(targetU, targetV, piece.Rotation);
                return MoveResult.Moved;
            }
            return state == CellState.Solid ? MoveResult.BlockedBySolid : MoveResult.BlockedByPiece;
        }

        public bool TryRotateClockwise(Piece piece)
        {
            int targetRotation = piece.Rotation + 1;
            for (int i = 0; i < RotationKicks.Length; i++)
            {
                int u = piece.U + RotationKicks[i].X;
                int v = piece.V + RotationKicks[i].Y;
                if (CheckPlacement(piece, u, v, targetRotation) == CellState.Empty)
                {
                    piece.SetPlacement(u, v, targetRotation);
                    return true;
                }
            }
            return false;
        }

        /// <summary>중력 방향으로 막히기 전까지 갈 수 있는 칸 수 (다른 낙하 피스도 막는 것으로 본다)</summary>
        public int DropDistance(Piece piece)
        {
            int maxDistance = _lanes.BoardSize * 2;
            int distance = 0;
            while (distance < maxDistance
                && CheckPlacement(piece, piece.U, piece.V - (distance + 1), piece.Rotation) == CellState.Empty)
            {
                distance++;
            }
            return distance;
        }

        public bool IsResting(Piece piece)
        {
            return CheckPlacement(piece, piece.U, piece.V - 1, piece.Rotation) == CellState.Solid;
        }

        /// <summary>
        /// 줄 정리 뒤 고정 블록과 겹친 피스를 중력 반대 방향으로 밀어낸다.
        /// 레인 밖까지 밀려나면 false.
        /// </summary>
        public bool TryPushOut(Piece piece)
        {
            for (int step = 0; step <= _lanes.Depth; step++)
            {
                if (CheckPlacement(piece, piece.U, piece.V + step, piece.Rotation) != CellState.Solid)
                {
                    piece.SetPlacement(piece.U, piece.V + step, piece.Rotation);
                    return true;
                }
            }
            return false;
        }
    }
}
