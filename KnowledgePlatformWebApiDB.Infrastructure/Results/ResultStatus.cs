using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.Infrastructure.Results;

public enum ResultStatus { 
    Success,
    Created,
    Accepted,
    NotFound,
    ValidationError,
    Concurrency,
    Conflict,
    Unauthorized,
    Forbidden,
    Error
}
