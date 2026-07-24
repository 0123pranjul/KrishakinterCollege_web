using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.ViewModels;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class IdCardService : IIdCardService
    {
        private readonly LibmanagementContext _context;

        public IdCardService(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<List<IdCardTemplateDto>> GetAllTemplatesAsync()
        {
            return await _context.TblIdCardTemplates
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new IdCardTemplateDto
                {
                    TemplateId = t.TemplateId,
                    TemplateName = t.TemplateName,
                    Orientation = t.Orientation,
                    SchoolName = t.SchoolName,
                    ThemeColor = t.ThemeColor,
                    IsActive = t.IsActive,
                    CreatedDate = t.CreatedDate
                }).ToListAsync();
        }

        public async Task<IdCardTemplateDto?> GetTemplateByIdAsync(int id)
        {
            var t = await _context.TblIdCardTemplates.FindAsync(id);
            if (t == null) return null;

            return new IdCardTemplateDto
            {
                TemplateId = t.TemplateId,
                TemplateName = t.TemplateName,
                Orientation = t.Orientation,
                SchoolName = t.SchoolName,
                SchoolAddress = t.SchoolAddress,
                SchoolContact = t.SchoolContact,
                ThemeColor = t.ThemeColor,
                BackgroundFrontPath = t.BackgroundFrontPath,
                BackgroundBackPath = t.BackgroundBackPath,
                SchoolLogoPath = t.SchoolLogoPath,
                PrincipalSignaturePath = t.PrincipalSignaturePath,
                FieldsConfigJson = t.FieldsConfigJson,
                IsActive = t.IsActive,
                CreatedDate = t.CreatedDate
            };
        }

        public async Task<int> SaveTemplateAsync(IdCardTemplateDto dto)
        {
            if (dto.TemplateId > 0)
            {
                var existing = await _context.TblIdCardTemplates.FindAsync(dto.TemplateId);
                if (existing == null) throw new Exception("Template not found");

                existing.TemplateName = dto.TemplateName;
                existing.Orientation = dto.Orientation;
                existing.SchoolName = dto.SchoolName;
                existing.SchoolAddress = dto.SchoolAddress;
                existing.SchoolContact = dto.SchoolContact;
                existing.ThemeColor = dto.ThemeColor;
                existing.FieldsConfigJson = dto.FieldsConfigJson;
                existing.IsActive = dto.IsActive;

                if (!string.IsNullOrEmpty(dto.BackgroundFrontPath)) existing.BackgroundFrontPath = dto.BackgroundFrontPath;
                if (!string.IsNullOrEmpty(dto.BackgroundBackPath)) existing.BackgroundBackPath = dto.BackgroundBackPath;
                if (!string.IsNullOrEmpty(dto.SchoolLogoPath)) existing.SchoolLogoPath = dto.SchoolLogoPath;
                if (!string.IsNullOrEmpty(dto.PrincipalSignaturePath)) existing.PrincipalSignaturePath = dto.PrincipalSignaturePath;

                await _context.SaveChangesAsync();
                return existing.TemplateId;
            }
            else
            {
                var template = new TblIdCardTemplate
                {
                    TemplateName = dto.TemplateName,
                    Orientation = dto.Orientation,
                    SchoolName = dto.SchoolName,
                    SchoolAddress = dto.SchoolAddress,
                    SchoolContact = dto.SchoolContact,
                    ThemeColor = dto.ThemeColor,
                    BackgroundFrontPath = dto.BackgroundFrontPath,
                    BackgroundBackPath = dto.BackgroundBackPath,
                    SchoolLogoPath = dto.SchoolLogoPath,
                    PrincipalSignaturePath = dto.PrincipalSignaturePath,
                    FieldsConfigJson = dto.FieldsConfigJson,
                    IsActive = dto.IsActive,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TblIdCardTemplates.Add(template);
                await _context.SaveChangesAsync();
                return template.TemplateId;
            }
        }

        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var template = await _context.TblIdCardTemplates.FindAsync(id);
            if (template != null)
            {
                _context.TblIdCardTemplates.Remove(template);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<SelectListItemDto>> GetActiveClassesAsync()
        {
            return await _context.TblClasses
                .Where(c => c.IsActive == true)
                .Select(c => new SelectListItemDto { Id = c.ClassId, Name = c.ClassName ?? "Unknown Class" })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<SelectListItemDto>> GetSectionsByClassAsync(int classId)
        {
            var sections = await _context.TblClassSections
                .Where(cs => cs.ClassId == classId)
                .Select(cs => cs.SectionId)
                .ToListAsync();

            if (!sections.Any()) return new List<SelectListItemDto>();

            return await _context.TblSections
                .Where(s => sections.Contains(s.SectionId) && s.IsActive == true)
                .Select(s => new SelectListItemDto { Id = s.SectionId, Name = s.SectionName ?? "Unknown Section" })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<IdCardStudentDto>> GetStudentsForIdCardAsync(int? classId, int? sectionId, string? searchQuery)
        {
            var query = _context.TblStudentSessions
                .Include(ss => ss.Student)
                .Include(ss => ss.Class)
                .Include(ss => ss.Section)
                .Where(ss => ss.IsActive == true && ss.Student != null && ss.Student.IsActive == true);

            if (classId.HasValue && classId.Value > 0)
            {
                query = query.Where(ss => ss.ClassId == classId.Value);
            }
            if (sectionId.HasValue && sectionId.Value > 0)
            {
                query = query.Where(ss => ss.SectionId == sectionId.Value);
            }
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.ToLower();
                query = query.Where(ss => (ss.Student.StudentName != null && ss.Student.StudentName.ToLower().Contains(q)) || 
                                          (ss.Student.RollNo != null && ss.Student.RollNo.ToLower().Contains(q)) || 
                                          (ss.Student.AdmissionNo != null && ss.Student.AdmissionNo.ToLower().Contains(q)));
            }

            var results = await query.Select(ss => new IdCardStudentDto
            {
                StudentId = ss.Student.StudentId,
                StudentName = ss.Student.StudentName ?? "N/A",
                RollNo = ss.Student.RollNo,
                AdmissionNo = ss.Student.AdmissionNo,
                ClassName = ss.Class != null ? ss.Class.ClassName : "N/A",
                SectionName = ss.Section != null ? ss.Section.SectionName : "N/A",
                DateOfBirth = ss.Student.DateOfBirth.HasValue ? ss.Student.DateOfBirth.Value.ToString("dd-MMM-yyyy") : "",
                BloodGroup = ss.Student.BloodGroup,
                Address = ss.Student.AddressLine1 + (string.IsNullOrEmpty(ss.Student.City) ? "" : ", " + ss.Student.City),
                FatherName = _context.TblStudentParents.Where(p => p.StudentId == ss.StudentId && p.ParentType == "Father").Select(p => p.ParentName).FirstOrDefault(),
                Phone = _context.TblStudentParents.Where(p => p.StudentId == ss.StudentId && p.ParentType == "Father").Select(p => p.MobileNo).FirstOrDefault(),
                PhotoPath = ss.Student.PhotoUrl

            }).OrderBy(x => x.ClassName).ThenBy(x => x.SectionName).ThenBy(x => x.StudentName).ToListAsync();

            return results;
        }
    }
}
