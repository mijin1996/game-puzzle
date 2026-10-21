using System.Collections.Generic;
using NUnit.Framework;
using Quadrop.Core;

namespace Quadrop.Tests
{
    public sealed class RulesTests
    {
        [Test]
        public void Score_NoLines_IsZero()
        {
            ScoreCalculator score = new ScoreCalculator(new[] { 100, 300 }, new[] { 1f, 2f });
            Assert.AreEqual(0, score.Calculate(0, 3, 1));
        }

        [Test]
        public void Score_MoreLinesThanTable_UsesLastEntry()
        {
            ScoreCalculator score = new ScoreCalculator(new[] { 100, 300 }, new[] { 1f });
            Assert.AreEqual(300, score.Calculate(5, 1, 1));
        }

        [Test]
        public void Score_TwoSidesLevelTwo_AppliesBothMultipliers()
        {
            ScoreCalculator score = new ScoreCalculator(new[] { 100 }, new[] { 1f, 1.5f });
            Assert.AreEqual(300, score.Calculate(1, 2, 2));
        }

        [Test]
        public void Gravity_HighLevel_NeverBelowMinimum()
        {
            GravitySchedule schedule = new GravitySchedule(0.8f, 0.5f, 0.05f);
            Assert.AreEqual(0.8f, schedule.FallSeconds(1), 0.0001f);
            Assert.AreEqual(0.05f, schedule.FallSeconds(50), 0.0001f);
        }

        [Test]
        public void SevenBag_FirstSeven_ContainEveryTypeOnce()
        {
            SevenBagSequence sequence = new SevenBagSequence(new FixedRandomSource(0));
            HashSet<TetrominoType> seen = new HashSet<TetrominoType>();
            for (int i = 0; i < TetrominoShapes.TypeCount; i++)
            {
                Assert.IsTrue(seen.Add(sequence.Next()));
            }
        }

        [Test]
        public void SevenBag_Peek_MatchesNext()
        {
            SevenBagSequence sequence = new SevenBagSequence(new FixedRandomSource(3));
            TetrominoType peeked = sequence.Peek(2);
            sequence.Next();
            sequence.Next();
            Assert.AreEqual(peeked, sequence.Next());
        }

        [Test]
        public void StandardProgression_Thresholds_UnlockInOrder()
        {
            StandardProgression rule = new StandardProgression(
                new[] { 0, 10, 25, 45 }, new[] { Side.Top, Side.Left, Side.Bottom, Side.Right });

            Assert.AreEqual(SideSet.Of(Side.Top), rule.ActiveSidesFor(9));
            Assert.AreEqual(SideSet.Of(Side.Top).With(Side.Left), rule.ActiveSidesFor(10));
            Assert.AreEqual(SideSet.All, rule.ActiveSidesFor(100));
            Assert.AreEqual(25, rule.NextUnlockAt(10));
            Assert.AreEqual(-1, rule.NextUnlockAt(45));
        }

        [Test]
        public void StandardProgression_FirstThresholdNotZero_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new StandardProgression(
                new[] { 1 }, new[] { Side.Top }));
        }

        [Test]
        public void AllSidesProgression_Always_AllSides()
        {
            Assert.AreEqual(SideSet.All, new AllSidesProgression().ActiveSidesFor(0));
        }

        [Test]
        public void GameRules_Defaults_AreValid()
        {
            Assert.IsTrue(new GameRules().Validate().IsSuccess);
        }

        [Test]
        public void GameRules_LaneTooWideForSpawnMargin_Fails()
        {
            GameRules rules = new GameRules();
            rules.BoardSize = 20;
            rules.LaneWidth = 14;
            Assert.IsFalse(rules.Validate().IsSuccess);
        }

        [Test]
        public void GameRules_OddBoardSize_Fails()
        {
            GameRules rules = new GameRules();
            rules.BoardSize = 21;
            Assert.IsFalse(rules.Validate().IsSuccess);
        }

        [Test]
        public void GameRules_DuplicateUnlockSide_Fails()
        {
            GameRules rules = new GameRules();
            rules.UnlockOrder = new[] { Side.Top, Side.Top, Side.Bottom, Side.Right };
            Assert.IsFalse(rules.Validate().IsSuccess);
        }
    }
}
