using KnowledgePlatformWebApiDB.Infrastructure.Results;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    public IActionResult HandleResult(Result result)
    {
        return result.Status switch
        {
            ResultStatus.Success => NoContent(),
            ResultStatus.Created => StatusCode(StatusCodes.Status201Created),
            ResultStatus.Accepted => Accepted(),
            ResultStatus.NotFound => NotFound(CreateProblemDetails(result)),
            ResultStatus.Concurrency => StatusCode(StatusCodes.Status412PreconditionFailed, CreateProblemDetails(result)),
            ResultStatus.Conflict => Conflict(CreateProblemDetails(result)),
            ResultStatus.ValidationError => BadRequest(CreateValidationProblemDetails(result)),
            ResultStatus.Unauthorized => Unauthorized(CreateProblemDetails(result)),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails(result)),

            _ => StatusCode(StatusCodes.Status500InternalServerError,
                    CreateProblemDetails(
                        result: result,
                        title: "Unexpected error",
                        detail: "An unexpected server error occurred."))

        };

    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Success => Ok(result.Data),
                ResultStatus.Created => Created(string.Empty, result.Data),
                ResultStatus.Accepted => Accepted(result.Data),
                _ => Ok(result.Data)
            };
        }

        // since failure, no data payload.
        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(CreateProblemDetails(result)),

            ResultStatus.Concurrency
                => StatusCode(StatusCodes.Status412PreconditionFailed,
                    CreateProblemDetails(
                        result: result,
                        title: "Concurrency conflict",
                        detail: "The information was modified by another user. Please reload and try again.")),

            ResultStatus.Conflict => Conflict(CreateProblemDetails(result)),
            ResultStatus.ValidationError => BadRequest(CreateValidationProblemDetails(result)),
            ResultStatus.Unauthorized => Unauthorized(CreateProblemDetails(result)),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails(result)),

            _ => StatusCode(StatusCodes.Status500InternalServerError,
                    CreateProblemDetails(
                        result: result,
                        title: "Unexpected error",
                        detail: "An unexpected server error occurred."))
        };
    }

    protected ProblemDetails CreateProblemDetails(Result result, string? title = null, string? detail = null)
    {
        var error = result.Errors.FirstOrDefault();
        return new ProblemDetails
        {
            Title = title ?? error?.Code ?? "Error",
            Detail = detail ?? error?.Message,
            Status = MapStatusCode(result.Status),
            Instance = HttpContext.Request.Path
        };
    }

    protected ProblemDetails CreateProblemDetails<T>(Result<T> result, string? title = null, string? detail = null) {
        var error = result.Errors.FirstOrDefault();
        return new ProblemDetails
        {
            Title = title ?? error?.Code ?? "Error",
            Detail = detail ?? error?.Message,
            Status = MapStatusCode(result.Status),
            Instance = HttpContext.Request.Path
        };
    }

    protected ValidationProblemDetails CreateValidationProblemDetails(Result result)
    {
        var errors = result.ValidationErrors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path
        };
    }


    protected ValidationProblemDetails CreateValidationProblemDetails<T>(Result<T> result)
    {
        var errors = result.ValidationErrors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path
        };
    }

    private static int MapStatusCode(ResultStatus status) =>
        status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            ResultStatus.Concurrency => StatusCodes.Status412PreconditionFailed,
            ResultStatus.Conflict => StatusCodes.Status409Conflict,
            ResultStatus.ValidationError => StatusCodes.Status400BadRequest,
            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
}
