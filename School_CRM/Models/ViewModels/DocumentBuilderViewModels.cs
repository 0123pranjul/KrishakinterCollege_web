namespace School_CRM.Models.ViewModels;

public class DocumentBuilderIndexVM
{
    public int? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentType { get; set; }
    public string? ComponentsJson { get; set; }
    public string? PrintSettingsJson { get; set; }
    public List<TemplateListItemDto> Templates { get; set; } = new();
    
    public List<SelectListItemDto> Classes { get; set; } = new();
    public List<SelectListItemDto> Subjects { get; set; } = new();
}

public class TemplateListItemDto
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = null!;
    public string TemplateType { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class MyDocumentsVM
{
    public List<DocumentListItemDto> Documents { get; set; } = new();
    public string? FilterType { get; set; }
}

public class DocumentListItemDto
{
    public int DocumentId { get; set; }
    public string DocumentName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? TemplateName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaveDocumentDto
{
    public int? DocumentId { get; set; }
    public string DocumentName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public int? TemplateId { get; set; }
    public string ComponentsJson { get; set; } = null!;
    public string? PrintSettingsJson { get; set; }
    public string Status { get; set; } = "Draft";
}

public class SelectListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class QuestionBankVM
{
    public List<QuestionDto> Questions { get; set; } = new();
    public string? FilterType { get; set; }
    public int? FilterClassId { get; set; }
    public int? FilterSubjectId { get; set; }
    
    public List<SelectListItemDto> Classes { get; set; } = new();
    public List<SelectListItemDto> Subjects { get; set; } = new();
}

public class QuestionDto
{
    public int QuestionId { get; set; }
    public int DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionType { get; set; } = null!;
    public string QuestionText { get; set; } = null!;
    public string? OptionsJson { get; set; }
    public decimal Marks { get; set; }
    public string? Difficulty { get; set; }
    public int? AnswerSpace { get; set; }
    public string? ImagePath { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ClassId { get; set; }
    public int? SubjectId { get; set; }
    public string? ClassName { get; set; }
    public string? SubjectName { get; set; }
}
