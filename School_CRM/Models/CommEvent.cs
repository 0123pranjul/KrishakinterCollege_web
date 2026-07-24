using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommEvent
{
    public int EventId { get; set; }

    public string EventTitle { get; set; } = null!;

    public string? Description { get; set; }

    public string EventType { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public bool IsFullDay { get; set; }

    public string? Venue { get; set; }

    public string TargetType { get; set; } = null!;

    public int? TargetClassId { get; set; }

    public string Color { get; set; } = null!;

    public bool IsPublished { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
