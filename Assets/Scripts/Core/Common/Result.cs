namespace Quadrop.Core
{
    // 패턴: Result — 실패를 예외 대신 값으로 돌려줘서 호출부가 처리를 빠뜨리지 않게
    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Ok(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Fail(string error)
        {
            return new Result<T>(false, default, error);
        }

        public override string ToString()
        {
            return IsSuccess ? $"Ok({Value})" : $"Fail({Error})";
        }
    }
}
