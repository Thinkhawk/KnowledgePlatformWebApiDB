using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.Infrastructure.Results;

public sealed class Result : ResultBase { 
    private Result(ResultStatus status, IEnumerable<ErrorModel>? errors = null, IEnumerable<ValidationErrorModel>? validationErrors = null)
        :base(status, errors, validationErrors) { }


    public static Result Success() => new(ResultStatus.Success);
    public static Result Created() => new(ResultStatus.Created);
    public static Result Accepted() => new(ResultStatus.Accepted);

    public static Result NotFound(string message) =>
        new(ResultStatus.NotFound,
            errors: new[] {
                new ErrorModel("NotFound", message)
            });

    public static Result Concurrency(string message) =>
        new(ResultStatus.Concurrency,
            errors: new[] {
                 new ErrorModel("Concurrency", message)
            });

    public static Result Conflict(string message) =>
        new(ResultStatus.Conflict,
            errors: new[] {
                 new ErrorModel("Conflict", message)
            });

    public static Result ValidationFailure(IEnumerable<ValidationErrorModel> validationErrors) =>
        new(ResultStatus.ValidationError, validationErrors: validationErrors);

    public static Result Unauthorized(string message) =>
        new(ResultStatus.Unauthorized,
            errors: new[]
            {
                    new ErrorModel("Unauthorized", message)
            });

    public static Result Forbidden(string message) =>
        new(ResultStatus.Forbidden,
            errors: new[]
            {
                    new ErrorModel("Forbidden", message)
            });

    public static Result Error(string message) =>
        new(ResultStatus.Error,
            errors: new[]
            {
                    new ErrorModel("Error", message)
            });
}

