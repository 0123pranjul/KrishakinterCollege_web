using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class DocBuilderQuestion
{
    public int QuestionId { get; set; }

    public int DocumentId { get; set; }

    public int QuestionNumber { get; set; }

    public string QuestionType { get; set; } = null!;

    public string QuestionText { get; set; } = null!;

    public string? OptionsJson { get; set; }

    public decimal Marks { get; set; }

    public string? Difficulty { get; set; }

    public int? AnswerSpace { get; set; }

    public string? ImagePath { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ClassId { get; set; }

    public int? SubjectId { get; set; }

    public virtual DocBuilderDocument Document { get; set; } = null!;
}
