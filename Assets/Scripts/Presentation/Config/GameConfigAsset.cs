using Quadrop.Core;
using Quadrop.Game;
using Quadrop.Infrastructure;
using UnityEngine;

namespace Quadrop.Presentation
{
    /// <summary>
    /// 밸런스 값을 Inspector에서 고치기 위한 ScriptableObject.
    /// Assets/Resources/GameConfig.asset 으로 만들면 자동으로 쓰이고, 없으면 아래 기본값을 쓴다.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Quadrop/Game Config")]
    public sealed class GameConfigAsset : ScriptableObject
    {
        public const string ResourcePath = "GameConfig";

        [Header("Mode")]
        [SerializeField] private GameMode _mode = GameMode.Standard;

        [Header("Board")]
        [Tooltip("짝수. 레인 깊이 = 판 크기 / 2")]
        [SerializeField, Range(GameRules.MinBoardSize, GameRules.MaxBoardSize)] private int _boardSize = 20;
        [Tooltip("짝수. (판 크기 - 레인 폭) / 2 가 4 이상이어야 한다")]
        [SerializeField, Range(GameRules.MinLaneWidth, GameRules.MaxBoardSize)] private int _laneWidth = 10;

        [Header("Falling")]
        [SerializeField, Min(0.01f)] private float _baseFallSeconds = 0.8f;
        [SerializeField, Range(0.5f, 1f)] private float _fallSpeedFactor = 0.85f;
        [SerializeField, Min(0.01f)] private float _minFallSeconds = 0.05f;
        [SerializeField, Min(0.01f)] private float _softDropSeconds = 0.03f;
        [SerializeField, Min(0f)] private float _lockDelaySeconds = 0.5f;
        [SerializeField, Min(0)] private int _maxLockResets = 15;

        [Header("Flow")]
        [SerializeField, Min(0f)] private float _clearDelaySeconds = 0.3f;
        [SerializeField, Min(0f)] private float _unlockDelaySeconds = 1.2f;
        [SerializeField, Min(0f)] private float _boardRotateSeconds = 0.15f;
        [SerializeField, Min(1)] private int _linesPerLevel = 10;
        [SerializeField, Range(0, GameRules.MaxPreviewCount)] private int _nextPreviewCount = 3;

        [Header("Side unlocks (STANDARD)")]
        [SerializeField] private int[] _unlockThresholds = { 0, 10, 25, 45 };
        [SerializeField] private Side[] _unlockOrder = { Side.Top, Side.Left, Side.Bottom, Side.Right };

        [Header("Score")]
        [SerializeField] private int[] _lineScores = { 100, 300, 500, 800 };
        [SerializeField] private float[] _sideMultipliers = { 1f, 1.5f, 2f, 3f };

        [Header("Input")]
        [SerializeField, Min(0.01f)] private float _dasSeconds = 0.17f;
        [SerializeField, Min(0.01f)] private float _arrSeconds = 0.05f;

        public GameMode Mode => _mode;

        public GameRules ToRules()
        {
            GameRules rules = new GameRules();
            rules.BoardSize = _boardSize;
            rules.LaneWidth = _laneWidth;
            rules.BaseFallSeconds = _baseFallSeconds;
            rules.FallSpeedFactor = _fallSpeedFactor;
            rules.MinFallSeconds = _minFallSeconds;
            rules.SoftDropSeconds = _softDropSeconds;
            rules.LockDelaySeconds = _lockDelaySeconds;
            rules.MaxLockResets = _maxLockResets;
            rules.ClearDelaySeconds = _clearDelaySeconds;
            rules.UnlockDelaySeconds = _unlockDelaySeconds;
            rules.BoardRotateSeconds = _boardRotateSeconds;
            rules.LinesPerLevel = _linesPerLevel;
            rules.NextPreviewCount = _nextPreviewCount;
            rules.UnlockThresholds = (int[])_unlockThresholds.Clone();
            rules.UnlockOrder = (Side[])_unlockOrder.Clone();
            rules.LineScores = (int[])_lineScores.Clone();
            rules.SideMultipliers = (float[])_sideMultipliers.Clone();
            return rules;
        }

        public InputTuning ToInputTuning()
        {
            return new InputTuning(_dasSeconds, _arrSeconds);
        }
    }
}
