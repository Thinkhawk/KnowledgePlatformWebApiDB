namespace KnowledgePlatformWebApiDB.Infrastructure.Results;

public class ResultBase
{
    public ResultStatus Status { get; }
    public IReadOnlyList<ErrorModel> Errors { get; }
    public IReadOnlyList<ValidationErrorModel> ValidationErrors { get; }
    public bool IsSuccess => Status is ResultStatus.Success or ResultStatus.Created or ResultStatus.Accepted;

    protected ResultBase(ResultStatus status, IEnumerable<ErrorModel>? errors = null, IEnumerable<ValidationErrorModel>? validationErrors = null)
    { 
        Status = status;
        Errors = errors?.ToList() ?? new List<ErrorModel>();
        ValidationErrors = validationErrors.ToList() ?? new List<ValidationErrorModel>();
    }
}
