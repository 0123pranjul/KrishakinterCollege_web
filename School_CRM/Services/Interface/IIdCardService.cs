using School_CRM.Models;
using School_CRM.Models.ViewModels;

namespace School_CRM.Services.Interface
{
    public interface IIdCardService
    {
        // Template Management
        Task<List<IdCardTemplateDto>> GetAllTemplatesAsync();
        Task<IdCardTemplateDto?> GetTemplateByIdAsync(int id);
        Task<int> SaveTemplateAsync(IdCardTemplateDto dto);
        Task<bool> DeleteTemplateAsync(int id);

        // Student Data Retrieval for ID Cards
        Task<List<SelectListItemDto>> GetActiveClassesAsync();
        Task<List<SelectListItemDto>> GetSectionsByClassAsync(int classId);
        Task<List<IdCardStudentDto>> GetStudentsForIdCardAsync(int? classId, int? sectionId, string? searchQuery);
    }
}
