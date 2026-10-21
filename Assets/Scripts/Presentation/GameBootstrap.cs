using System;
using Quadrop.Core;
using Quadrop.Game;
using Quadrop.Infrastructure;
using UnityEngine;

namespace Quadrop.Presentation
{
    // 패턴: Composition Root — 객체 생성과 연결을 한 곳에 모아서, 나머지 클래스는 인터페이스만 알게 하려고
    /// <summary>
    /// 게임 전체를 조립한다. 씬에 이 컴포넌트가 없으면 씬 로드 직후 자동으로 하나 만든다
    /// (아무 씬에서나 Play를 누르면 바로 플레이 가능).
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const float CameraMargin = 2.5f;
        private const float CameraDepth = -10f;
        private const float PreviewScale = 0.6f;
        private const float PreviewInsetX = 2.6f;
        private const float PreviewInsetY = 3.2f;

        [SerializeField] private GameConfigAsset _config;

        private GameSession _session;
        private InputReader _input;
        private HighScoreTracker _highScore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null)
            {
                return;
            }
            new GameObject("Quadrop").AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            GameConfigAsset config = LoadConfig();
            GameRules rules = config.ToRules();
            Result<GameRules> validation = rules.Validate();
            if (!validation.IsSuccess)
            {
                Debug.LogError($"[Quadrop] Invalid GameConfig, using defaults: {validation.Error}");
                rules = new GameRules();
            }

            IProgressionRule progression = ProgressionFactory.Create(config.Mode, rules);
            IPieceSequence sequence = new SevenBagSequence(new SystemRandomSource(Environment.TickCount));
            _session = new GameSession(rules, progression, sequence);

            _highScore = new HighScoreTracker(_session.Events, _session.Stats, new PlayerPrefsHighScoreRepository());
            _highScore.Load();
            _input = new InputReader(_session, config.ToInputTuning());

            Camera mainCamera = SetupCamera(rules.BoardSize);
            Sprite cellSprite = SpriteFactory.CreateCellSprite();

            BoardView board = CreateChild<BoardView>("Board");
            board.Construct(_session, cellSprite, rules.BoardRotateSeconds);

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            Vector3 holdCenter = new Vector3(-halfWidth + PreviewInsetX, halfHeight - PreviewInsetY, 0f);
            Vector3 nextTop = new Vector3(halfWidth - PreviewInsetX, halfHeight - PreviewInsetY, 0f);
            PreviewView previews = CreateChild<PreviewView>("Previews");
            previews.Construct(_session, cellSprite, holdCenter, nextTop, PreviewScale);

            HudView hud = CreateChild<HudView>("Hud");
            hud.Construct(_session, _highScore);

            GameLoopDriver driver = gameObject.AddComponent<GameLoopDriver>();
            driver.Construct(_session, _input);

            _session.Start();
        }

        private void OnDestroy()
        {
            _input?.Dispose();
            _highScore?.Dispose();
        }

        private GameConfigAsset LoadConfig()
        {
            if (_config != null)
            {
                return _config;
            }

            GameConfigAsset loaded = Resources.Load<GameConfigAsset>(GameConfigAsset.ResourcePath);
            return loaded != null ? loaded : ScriptableObject.CreateInstance<GameConfigAsset>();
        }

        private static Camera SetupCamera(int boardSize)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = boardSize * 0.5f + CameraMargin;
            mainCamera.transform.position = new Vector3(0f, 0f, CameraDepth);
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = BlockPalette.Backdrop;
            return mainCamera;
        }

        private T CreateChild<T>(string objectName) where T : Component
        {
            GameObject child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            return child.AddComponent<T>();
        }
    }
}
