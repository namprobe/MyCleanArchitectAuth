namespace Auth.Application.Common.Models
{
    public class Result<T>
    {
        internal Result(bool isSuccess, IEnumerable<string> errors, T data)
        {
            IsSuccess = isSuccess;
            Errors = errors.ToArray();
            Data = data;
        }

        public bool IsSuccess { get; }
        public string[] Errors { get; }
        public T Data { get; }
        public static Result<T> Success(T data)
        {
            return new Result<T>(true, Array.Empty<string>(), data);
        }

        public static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, errors, default!);
        }
    }

    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
        }

        public bool Succeeded { get; }
        public string[] Errors { get; }

        public static Result Success()
        {
            return new Result(true, Array.Empty<string>());
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }

        public static Result Failure(string error)
        {
            return new Result(false, new[] { error });
        }

        // Convert to generic Result
        public Result<T> ToGeneric<T>(T data)
        {
            return new Result<T>(Succeeded, Errors, data);
        }
    }
}
