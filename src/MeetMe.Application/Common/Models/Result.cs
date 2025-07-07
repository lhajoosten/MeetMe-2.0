using System.Diagnostics.CodeAnalysis;

namespace MeetMe.Application.Common.Models
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail
    /// </summary>
    public class Result
    {
        protected Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
        public static Result<T> Failure<T>(string error) => new(default, false, error);

        public static implicit operator Result(string error) => Failure(error);
    }

    /// <summary>
    /// Represents the result of an operation that returns a value
    /// </summary>
    /// <typeparam name="T">The type of the returned value</typeparam>
    public class Result<T> : Result
    {
        private readonly T? _value;

        protected internal Result(T? value, bool isSuccess, string error) : base(isSuccess, error)
        {
            _value = value;
        }

        [NotNull]
        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of failed result");

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(string error) => Failure<T>(error);
    }
}
