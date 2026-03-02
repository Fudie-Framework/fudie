namespace Fudie.Domain;

/// <summary>
/// Represents a validation error associated with a specific property.
/// </summary>
/// <param name="errorMessage">The error message describing the validation failure.</param>
/// <param name="propertyName">The name of the property that failed validation.</param>
public class ValidationError(string errorMessage, string propertyName = "")
{
    /// <summary>
    /// Gets the name of the property that caused the validation error.
    /// </summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// Gets the error message describing the validation failure.
    /// </summary>
    public string ErrorMessage { get; } = errorMessage;
}

/// <summary>
/// Represents the outcome of a domain operation that can succeed or fail with validation errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the validation errors associated with a failed result.
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="errors">Optional validation errors for a failed result.</param>
    protected Result(bool isSuccess, IEnumerable<ValidationError>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<ValidationError>();
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="propertyName">The property that caused the error.</param>
    public static Result Failure(string error, string propertyName = "")
        => new(false, [new ValidationError(error, propertyName)]);

    /// <summary>
    /// Creates a failed result with multiple validation errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public static Result Failure(IEnumerable<ValidationError> errors)
        => new(false, errors);
}

/// <summary>
/// Represents the outcome of a domain operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value returned by a successful operation, or <c>default</c> if the operation failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="errors">Optional validation errors for a failed result.</param>
    protected Result(T? value, bool isSuccess, IEnumerable<ValidationError>? errors = null)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The result value.</param>
    public static Result<T> Success(T value) => new(value, true);

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="propertyName">The property that caused the error.</param>
    public new static Result<T> Failure(string error, string propertyName = "")
        => new(default, false, [new ValidationError(error, propertyName)]);

    /// <summary>
    /// Creates a failed result with multiple validation errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public new static Result<T> Failure(IEnumerable<ValidationError> errors)
        => new(default, false, errors);
}
