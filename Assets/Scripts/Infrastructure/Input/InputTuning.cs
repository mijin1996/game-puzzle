namespace Quadrop.Infrastructure
{
    /// <summary>좌우 자동 반복 감각 값 (DAS = 반복 시작까지, ARR = 반복 간격)</summary>
    public sealed class InputTuning
    {
        public const float MinRepeatSeconds = 0.01f;

        public float DasSeconds { get; }
        public float ArrSeconds { get; }

        public InputTuning(float dasSeconds, float arrSeconds)
        {
            DasSeconds = dasSeconds < MinRepeatSeconds ? MinRepeatSeconds : dasSeconds;
            ArrSeconds = arrSeconds < MinRepeatSeconds ? MinRepeatSeconds : arrSeconds;
        }
    }
}
