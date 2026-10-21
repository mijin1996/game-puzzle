using System;

namespace Quadrop.Core
{
    /// <summary>레벨에 따른 한 칸 낙하 간격(초)</summary>
    public sealed class GravitySchedule
    {
        private readonly float _baseSeconds;
        private readonly float _speedFactor;
        private readonly float _minSeconds;

        public GravitySchedule(float baseSeconds, float speedFactor, float minSeconds)
        {
            if (baseSeconds <= 0f || minSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseSeconds), "Fall intervals must be positive");
            }
            if (speedFactor <= 0f || speedFactor > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(speedFactor), "Speed factor must be in (0, 1]");
            }

            _baseSeconds = baseSeconds;
            _speedFactor = speedFactor;
            _minSeconds = minSeconds;
        }

        public float FallSeconds(int level)
        {
            int steps = Math.Max(0, level - 1);
            return MathF.Max(_minSeconds, _baseSeconds * MathF.Pow(_speedFactor, steps));
        }
    }
}
