namespace KnowledgePlatformWebApiDB.Infrastructure.Results;

public sealed class Result<T>
    : ResultBase
{
    public T? Data { get; }

    private Result(ResultStatus status, T? data = default, IEnumerable<ErrorModel>? errors = null, IEnumerable<ValidationErrorModel>? validationErrors = null)
        :base(status, errors, validationErrors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(ResultStatus.Success, data);
    public static Result<T> Created(T data) => new(ResultStatus.Created, data);
    public static Result<T> Accepted(T data) => new(ResultStatus.Accepted, data);

    public static Result<T> NotFound(string message) =>
        new(ResultStatus.NotFound,
            errors: new[] {
                new ErrorModel("NotFound", message)
            });

    public static Result<T> Concurrency(string message) => 
        new(ResultStatus.Concurrency,
            errors: new[] {
                 new ErrorModel("Concurrency", message)
            });

    public static Result<T> Conflict(string message) => 
        new(ResultStatus.Conflict,
            errors: new[] {
                 new ErrorModel("Conflict", message)
            });

    public static Result<T> ValidationFailure(IEnumerable<ValidationErrorModel> validationErrors) =>
        new(ResultStatus.ValidationError, validationErrors: validationErrors);

    public static Result<T> Unauthorized(string message) =>
        new(ResultStatus.Unauthorized,
            errors: new[]
            {
                    new ErrorModel("Unauthorized", message)
            });

    public static Result<T> Forbidden(string message) =>
        new(ResultStatus.Forbidden,
            errors: new[]
            {
                    new ErrorModel("Forbidden", message)
            });

    public static Result<T> Error(string message) =>
        new(ResultStatus.Error,
            errors: new[]
            {
                    new ErrorModel("Error", message)
            });
}
