using System.Diagnostics.CodeAnalysis;

namespace Base.Domain.Result;

/// <summary>
/// Represents the result of an operation that can either succeed or fail without returning a value.
/// </summary>
public readonly record struct Result
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    private readonly Error? _error;

    public Error Error =>
        _error ?? throw new InvalidOperationException("No error present, use 'IsSuccess' to check the result state.");

    private Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null || !isSuccess && error is null)
            throw new InvalidOperationException("Invalid result state.");
        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(string code, string description, ErrorType type = ErrorType.Failure)
    {
        return new(false, new Error(code, description, type));
    }

    /// <summary>
    /// Implicitly converts an error to a failure result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);
}


/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public readonly record struct Result<TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    private readonly TValue? _value;

    public TValue Value
    {
        get
        {
            if (_value is null)
                throw new InvalidOperationException("No value present, use 'IsSuccess' to check the result state.");
            return _value;
        }
    }

    private readonly Error? _error;

    public Error Error
    {
        get
        {
            if (_error is null)
                throw new InvalidOperationException("No error present, use 'IsSuccess' to check the result state.");
            return _error!.Value;
        }
    }

    private Result(bool isSuccess, TValue? value, Error? error)
    {
        if (isSuccess && error is not null || !isSuccess && error is null)
        {
            throw new InvalidOperationException("Invalid result state.");
        }
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<TValue> Success(TValue value) => new(true, value, null);

    public static Result<TValue> Failure(Error error) => new(false, default, error);

    public static Result<TValue> Failure(string code, string description, ErrorType type = ErrorType.Failure)
    {
        return new(false, default, new Error(code, description, type));
    }

    /// <summary>
    /// Implicitly converts a value to a success result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a failure result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    /// <summary>
    /// Explicitly converts a generic result to a non-generic result, discarding the value.
    /// </summary>
    public static explicit operator Result(Result<TValue> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    /// <summary>
    /// Safely unwraps the result by forcing execution of one of two provided functions,
    /// one for the success case and one for the failure case.
    /// </summary>
    /// <typeparam name="TResult">The return type of the provided functions.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is a success.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The value returned by the executed function.</returns>
    /// <example>
    /// <code>
    /// Result&lt;int> result = Result&lt;nt>.Success(100);
    /// string message = result.Match(
    ///     onSuccess: value => $"Success with value: {value}",
    ///     onFailure: error => $"Failure with code: {error.Code}");
    /// // message is "Success with value: 100"
    /// </code>
    /// </example>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    /// Transforms the value of a success result into a new result.
    /// If the original result is a failure, it propagates the error.
    /// </summary>
    /// <typeparam name="TOut">The type of the new success value.</typeparam>
    /// <param name="mapFunc">The function to apply to the success value.</param>
    /// <returns>A new Result containing either the transformed value or the original error.</returns>
    /// <example>
    /// <code>
    /// Result&lt;int> result = Result&lt;int>.Success(5);
    /// Result&lt;string> newResult = result.Map(intValue => $"The number is {intValue}");
    /// // newResult is a Success result with the value "The number is 5"
    /// </code>
    /// </example>
    public Result<TOut> Map<TOut>(Func<TValue, TOut> mapFunc) =>
        IsSuccess ? mapFunc(Value) : Error;

    /// <summary>
    /// Chains an operation that also returns a Result, allowing for a sequence of fallible operations.
    /// If the original result is a failure, it propagates the error and the next operation is not executed.
    /// </summary>
    /// <typeparam name="TOut">The success type of the result returned by the chained function.</typeparam>
    /// <param name="bindFunc">The function to execute, which takes the success value and returns a new Result.</param>
    /// <returns>The result of the chained function if the original result was a success, otherwise the original error.</returns>
    /// <example>
    /// <code>
    /// Result&lt;int> TryParse(string s) => int.TryParse(s, out var i) ? i : new Error("Parse", "Invalid number");
    /// Result&lt;string> inputResult = Result&lt;string>.Success("123");
    /// Result&lt;int> finalResult = inputResult.Bind(TryParse);
    /// // finalResult is a Success result with the value 123
    /// </code>
    /// </example>
    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> bindFunc) =>
        IsSuccess ? bindFunc(Value) : Error;
}
