using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.Data.Entities;

public class AuditEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; }

    public string EntityName { get; set; }

    public string EntityId { get; set; }

    public string OldValues { get; set; }

    public string NewValues { get; set; }

    public DateTime Timestamp { get; set; }

    // Navigation Property

    public ApplicationUser User { get; set; }
}