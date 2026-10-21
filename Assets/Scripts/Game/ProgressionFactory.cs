using System;
using Quadrop.Core;

namespace Quadrop.Game
{
    // 패턴: Factory — 모드 enum에서 해금 규칙 구현체를 고르는 일을 한 곳에 모으려고
    public static class ProgressionFactory
    {
        public static IProgressionRule Create(GameMode mode, GameRules rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            switch (mode)
            {
                case GameMode.Standard:
                    return new StandardProgression(rules.UnlockThresholds, rules.UnlockOrder);
                case GameMode.AllSides:
                    return new AllSidesProgression();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}
