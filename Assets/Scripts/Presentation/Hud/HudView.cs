using System;
using System.Text;
using Quadrop.Core;
using Quadrop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrop.Presentation
{
    /// <summary>점수·레벨·안내 문구를 UGUI 텍스트로 표시한다. 글자는 값이 바뀔 때만 다시 만든다.</summary>
    public sealed class HudView : MonoBehaviour
    {
        private const string BuiltinFontName = "LegacyRuntime.ttf";
        private const int TotalSides = 4;
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 Margin = new Vector2(60f, 50f);

        private readonly StringBuilder _builder = new StringBuilder(160);

        private GameSession _session;
        private HighScoreTracker _highScore;
        private Text _stats;
        private Text _overlay;

        public void Construct(GameSession session, HighScoreTracker highScore)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _highScore = highScore ?? throw new ArgumentNullException(nameof(highScore));

            Font font = Resources.GetBuiltinResource<Font>(BuiltinFontName);
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 1f;

            CreateText("HoldLabel", font, 36, TextAnchor.UpperLeft, new Vector2(0f, 1f),
                new Vector2(Margin.x, -Margin.y), new Vector2(300f, 60f)).text = "HOLD";
            CreateText("NextLabel", font, 36, TextAnchor.UpperRight, new Vector2(1f, 1f),
                new Vector2(-Margin.x, -Margin.y), new Vector2(300f, 60f)).text = "NEXT";
            Text help = CreateText("Help", font, 24, TextAnchor.LowerCenter, new Vector2(0.5f, 0f),
                new Vector2(0f, Margin.y * 0.5f), new Vector2(1800f, 60f));
            help.color = BlockPalette.TextDim;
            help.text = "Arrows move / soft drop   Z or Up rotate   Space hard drop   C hold   A / D rotate board   Esc pause   R restart";
            _stats = CreateText("Stats", font, 34, TextAnchor.UpperLeft, new Vector2(0f, 0.5f),
                new Vector2(Margin.x, 120f), new Vector2(420f, 520f));
            _overlay = CreateText("Overlay", font, 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1400f, 400f));
            _overlay.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3f, -3f);

            _session.Events.StatsChanged += RefreshStats;
            _session.Events.SideUnlocked += OnSideUnlocked;
            _session.Events.StateChanged += OnStateChanged;
            _highScore.BestChanged += RefreshStats;
            RefreshStats();
        }

        private void OnDestroy()
        {
            if (_session != null)
            {
                _session.Events.StatsChanged -= RefreshStats;
                _session.Events.SideUnlocked -= OnSideUnlocked;
                _session.Events.StateChanged -= OnStateChanged;
            }
            if (_highScore != null)
            {
                _highScore.BestChanged -= RefreshStats;
            }
        }

        private void OnSideUnlocked(Side side)
        {
            RefreshStats();
        }

        private void RefreshStats()
        {
            GameStats stats = _session.Stats;
            _builder.Clear();
            _builder.Append("SCORE\n").Append(stats.Score).Append("\n\n");
            _builder.Append("LINES   ").Append(stats.Lines).Append('\n');
            _builder.Append("LEVEL   ").Append(stats.Level).Append('\n');
            _builder.Append("BEST    ").Append(Math.Max(_highScore.Best, stats.Score)).Append("\n\n");
            _builder.Append("SIDES   ").Append(_session.ActiveSides.Count).Append(" / ").Append(TotalSides).Append('\n');

            int nextUnlock = _session.NextUnlockAtLines;
            if (nextUnlock >= 0)
            {
                _builder.Append("NEXT SIDE AT ").Append(nextUnlock).Append(" LINES");
            }
            else
            {
                _builder.Append("ALL SIDES OPEN");
            }
            _stats.text = _builder.ToString();
        }

        private void OnStateChanged(GameStateId state)
        {
            switch (state)
            {
                case GameStateId.Paused:
                    _overlay.text = "PAUSED\n<size=32>Esc resume    R restart</size>";
                    break;
                case GameStateId.GameOver:
                    _overlay.text = "GAME OVER\n<size=32>R restart</size>";
                    break;
                case GameStateId.Unlocking:
                    _overlay.text = "NEW SIDE OPEN!\n<size=32>A / D to rotate the board</size>";
                    break;
                default:
                    _overlay.text = string.Empty;
                    break;
            }
            RefreshStats();
        }

        private Text CreateText(string objectName, Font font, int fontSize, TextAnchor alignment, Vector2 anchor,
            Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            RectTransform rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = BlockPalette.Text;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }
    }
}
