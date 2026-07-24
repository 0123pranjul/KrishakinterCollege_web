using School_CRM.Models;
using School_CRM.Models.ViewModels;

namespace School_CRM.Services.Interface;

public interface IDocumentBuilderService
{
    Task<List<TemplateListItemDto>> GetTemplatesAsync();
    Task<DocBuilderTemplate?> GetTemplateByIdAsync(int id);
    Task<List<DocumentListItemDto>> GetDocumentsAsync(int userId, string? filterType = null);
    Task<DocBuilderDocument?> GetDocumentByIdAsync(int id);
    Task<int> SaveDocumentAsync(SaveDocumentDto dto, int userId);
    Task<bool> DeleteDocumentAsync(int id, int userId);
    Task<DocBuilderImage> UploadImageAsync(IFormFile file, int? documentId, int userId);
    Task<List<QuestionDto>> GetQuestionsAsync(string? filterType = null, int? classId = null, int? subjectId = null);
    Task<int> SaveQuestionAsync(QuestionDto dto, int userId);
    Task<bool> DeleteQuestionAsync(int id);
    
    Task<List<SelectListItemDto>> GetActiveClassesAsync();
    Task<List<SelectListItemDto>> GetActiveSubjectsAsync();
}
