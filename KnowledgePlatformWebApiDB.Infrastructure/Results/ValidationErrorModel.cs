namespace KnowledgePlatformWebApiDB.Infrastructure.Results;

public sealed record class ValidationErrorModel(string PropertyName, string ErrorMessage);
