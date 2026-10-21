using NUnit.Framework;
using Quadrop.Core;
using Quadrop.Game;

namespace Quadrop.Tests
{
    public sealed class GameFlowTests
    {
        private const float SmallStep = 0.01f;

        [Test]
        public void ControlState_OneSide_CannotRotate()
        {
            ControlState control = new ControlState();
            control.Reset(SideSet.Of(Side.Top));
            Assert.IsFalse(control.TryRotateBoard(true, SideSet.Of(Side.Top)));
            Assert.AreEqual(Side.Top, control.Controlled);
        }

        [Test]
        public void ControlState_RotateClockwise_BringsLeftSideUp()
        {
            SideSet active = SideSet.Of(Side.Top).With(Side.Left).With(Side.Bottom);
            ControlState control = new ControlState();
            control.Reset(active);

            Assert.IsTrue(control.TryRotateBoard(true, active));
            Assert.AreEqual(Side.Left, control.Controlled);
            Assert.IsTrue(control.TryRotateBoard(true, active));
            Assert.AreEqual(Side.Bottom, control.Controlled);
        }

        [Test]
        public void ControlState_RotateCounterClockwise_SkipsInactiveSides()
        {
            SideSet active = SideSet.Of(Side.Top).With(Side.Left);
            ControlState control = new ControlState();
            control.Reset(active);

            Assert.IsTrue(control.TryRotateBoard(false, active));
            Assert.AreEqual(Side.Left, control.Controlled);
        }

        [Test]
        public void HoldSlot_SecondHoldBeforeLock_Refused()
        {
            HoldSlot hold = new HoldSlot();
            Assert.IsTrue(hold.TryHold(TetrominoType.T, out TetrominoType first));
            Assert.AreEqual(TetrominoType.None, first);
            Assert.IsFalse(hold.TryHold(TetrominoType.S, out _));

            hold.AllowUse();
            Assert.IsTrue(hold.TryHold(TetrominoType.S, out TetrominoType released));
            Assert.AreEqual(TetrominoType.T, released);
        }

        [Test]
        public void Start_FirstTick_SpawnsTopPieceOnly()
        {
            GameSession session = CreateSession(TetrominoType.O, null);
            session.Start();
            session.Tick(SmallStep);

            GridPos[] cells = new GridPos[TetrominoShapes.CellCount];
            Assert.AreEqual(GameStateId.Playing, session.State);
            Assert.IsTrue(session.TryGetPieceCells(Side.Top, cells, out TetrominoType type));
            Assert.AreEqual(TetrominoType.O, type);
            Assert.IsFalse(session.TryGetPieceCells(Side.Left, cells, out _));
        }

        [Test]
        public void HardDrop_FillTwoRows_ClearsScoresAndUnlocksSides()
        {
            GameSession session = CreateSession(TetrominoType.O, new[] { 0, 1, 2, 3 });
            session.Start();
            session.Tick(SmallStep);

            DropO(session, GameCommand.MoveLeft, 1);
            DropO(session, GameCommand.MoveRight, 1);
            Assert.AreEqual(GameStateId.Clearing, session.State, "two rows of the 4-wide lane are full");

            session.Tick(TestRules.ShortDelay * 2f);

            Assert.AreEqual(2, session.Stats.Lines);
            Assert.AreEqual(300, session.Stats.Score);
            Assert.AreEqual(GameStateId.Unlocking, session.State);
            Assert.IsTrue(session.ActiveSides.Contains(Side.Left));
            Assert.IsTrue(session.ActiveSides.Contains(Side.Bottom));
            Assert.IsFalse(session.ActiveSides.Contains(Side.Right));
        }

        [Test]
        public void RotateBoard_SingleSide_StaysPlaying()
        {
            GameSession session = CreateSession(TetrominoType.O, null);
            session.Start();
            session.Tick(SmallStep);

            session.Enqueue(GameCommand.RotateBoardClockwise);
            session.Tick(SmallStep);
            Assert.AreEqual(GameStateId.Playing, session.State);
            Assert.AreEqual(Side.Top, session.ControlledSide);
        }

        [Test]
        public void SpawnBlocked_LockedBlockAtSpawn_GameOverThenRestart()
        {
            GameSession session = CreateSession(TetrominoType.O, null);
            session.Start();
            session.Context.Grid.Set(5, TestRules.BoardSize - 1, MockGrids.Block);
            session.Tick(SmallStep);
            Assert.AreEqual(GameStateId.GameOver, session.State);

            session.Enqueue(GameCommand.Restart);
            session.Tick(SmallStep);
            Assert.AreEqual(GameStateId.Playing, session.State);
            Assert.AreEqual(0, session.Stats.Score);
        }

        [Test]
        public void HardDrop_StackReachesLaneEdge_GameOver()
        {
            GameSession session = CreateSession(TetrominoType.O, null);
            session.Start();
            session.Tick(SmallStep);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(GameStateId.Playing, session.State, $"drop {i}");
                session.Enqueue(GameCommand.HardDrop);
                session.Tick(SmallStep);
            }
            Assert.AreEqual(GameStateId.GameOver, session.State);
            Assert.AreEqual(0, session.Stats.Lines);
        }

        [Test]
        public void Pause_Toggle_ReturnsToPlaying()
        {
            GameSession session = CreateSession(TetrominoType.O, null);
            session.Start();
            session.Enqueue(GameCommand.Pause);
            session.Tick(SmallStep);
            Assert.AreEqual(GameStateId.Paused, session.State);

            session.Enqueue(GameCommand.Pause);
            session.Tick(SmallStep);
            Assert.AreEqual(GameStateId.Playing, session.State);
        }

        [Test]
        public void HighScore_GameOverWithBetterScore_Saves()
        {
            GameEvents events = new GameEvents();
            GameStats stats = new GameStats(10);
            MockHighScoreRepository repository = new MockHighScoreRepository(100);
            HighScoreTracker tracker = new HighScoreTracker(events, stats, repository);
            tracker.Load();

            stats.AddClear(1, 500);
            events.RaiseStateChanged(GameStateId.GameOver);

            Assert.AreEqual(500, tracker.Best);
            Assert.AreEqual(500, repository.Stored);
            tracker.Dispose();
        }

        [Test]
        public void HighScore_SaveFails_KeepsErrorMessage()
        {
            GameEvents events = new GameEvents();
            GameStats stats = new GameStats(10);
            MockHighScoreRepository repository = new MockHighScoreRepository(null);
            repository.FailSaves = true;
            HighScoreTracker tracker = new HighScoreTracker(events, stats, repository);
            tracker.Load();

            stats.AddClear(1, 50);
            events.RaiseStateChanged(GameStateId.GameOver);

            Assert.AreEqual("disk full", tracker.LastError);
            tracker.Dispose();
        }

        private static GameSession CreateSession(TetrominoType type, int[] thresholds)
        {
            GameRules rules = TestRules.Create(thresholds);
            IProgressionRule progression = ProgressionFactory.Create(GameMode.Standard, rules);
            return new GameSession(rules, progression, new FixedPieceSequence(type));
        }

        private static void DropO(GameSession session, GameCommand move, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                session.Enqueue(move);
            }
            session.Enqueue(GameCommand.HardDrop);
            session.Tick(SmallStep);
        }
    }
}
