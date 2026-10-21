using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    /// <summary>
    /// 한 판에 필요한 객체를 묶어 상태들이 함께 쓰게 한다.
    /// 규칙 계산은 각 클래스에 있고, 여기에는 여러 상태가 같이 쓰는 짧은 흐름 함수만 둔다.
    /// </summary>
    internal sealed class GameContext
    {
        public GameRules Rules { get; }
        public GridModel Grid { get; }
        public BoardFrames Frames { get; }
        public LaneLayout Lanes { get; }
        public PieceMover Mover { get; }
        public SpawnerSet Spawners { get; }
        public ControlState Control { get; }
        public HoldSlot Hold { get; }
        public GameStats Stats { get; }
        public GameEvents Events { get; }
        public IPieceSequence Sequence { get; }
        public IProgressionRule Progression { get; }
        public LineClearer Clearer { get; }
        public ScoreCalculator Score { get; }
        public GravityStepper Gravity { get; }
        public PieceController Controller { get; }
        public GameStateMachine Machine { get; }

        public SideSet PendingUnlocks { get; set; }
        public bool SoftDropHeld { get; set; }

        public GameContext(GameRules rules, IProgressionRule progression, IPieceSequence sequence)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            Progression = progression ?? throw new ArgumentNullException(nameof(progression));
            Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));

            Grid = new GridModel(rules.BoardSize);
            Frames = new BoardFrames(rules.BoardSize);
            Lanes = new LaneLayout(Frames, rules.LaneWidth);
            Spawners = new SpawnerSet(rules.BoardSize);
            Mover = new PieceMover(new BoardCollision(Grid, Frames, Spawners), Lanes);
            Control = new ControlState();
            Hold = new HoldSlot();
            Stats = new GameStats(rules.LinesPerLevel);
            Events = new GameEvents();
            Clearer = new LineClearer(Grid, Lanes);
            Score = new ScoreCalculator(rules.LineScores, rules.SideMultipliers);

            PieceLocker locker = new PieceLocker(Grid, Frames, Spawners, Hold, Events);
            GravitySchedule schedule = new GravitySchedule(rules.BaseFallSeconds, rules.FallSpeedFactor, rules.MinFallSeconds);
            Gravity = new GravityStepper(Mover, Spawners, locker, schedule, Stats, rules);
            Controller = new PieceController(Mover, Spawners, Control, Hold, locker, Events, rules);

            Machine = new GameStateMachine(Events);
            Machine.Register(new PlayingState(this));
            Machine.Register(new RotatingState(this));
            Machine.Register(new ClearingState(this));
            Machine.Register(new UnlockingState(this));
            Machine.Register(new PausedState(this));
            Machine.Register(new GameOverState(this));
        }

        /// <summary>빈 변에 피스를 채우고 모든 피스를 떨어뜨린다 (Playing·Rotating 공통).</summary>
        public void AdvancePieces(float deltaSeconds)
        {
            if (Spawners.FillEmpty(Sequence, Mover, Events) == SpawnOutcome.Blocked)
            {
                Machine.Change(GameStateId.GameOver);
                return;
            }

            if (Gravity.Step(deltaSeconds, SoftDropHeld, Control.Controlled) > 0)
            {
                CheckLines();
            }
        }

        /// <summary>꽉 찬 줄이 있으면 표시하고 Clearing 상태로 넘어간다.</summary>
        public void CheckLines()
        {
            if (Clearer.MarkFullLines(Spawners.Active) == 0)
            {
                return;
            }
            Events.RaiseLinesMarked(Clearer.MarkedCells);
            Machine.Change(GameStateId.Clearing);
        }

        /// <summary>줄 정리 뒤 고정 블록과 겹친 피스를 밀어낸다. 못 빼내면 false.</summary>
        public bool PushOutActivePieces()
        {
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                if (Spawners.TryGetPiece((Side)i, out Piece piece) && !Mover.TryPushOut(piece))
                {
                    return false;
                }
            }
            return true;
        }

        public void ResetGame()
        {
            Grid.ClearAll();
            Sequence.Reset();
            Hold.Clear();
            Stats.Reset();
            SoftDropHeld = false;
            PendingUnlocks = SideSet.None;

            SideSet initial = Progression.ActiveSidesFor(0);
            Spawners.Reset(initial);
            Control.Reset(initial);

            Events.RaiseBoardChanged();
            Events.RaiseStatsChanged();
            Events.RaiseHoldChanged();
            Events.RaiseControlChanged(Control.Controlled);
            Machine.Change(GameStateId.Playing);
        }
    }
}
