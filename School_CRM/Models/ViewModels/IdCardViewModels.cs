using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using School_CRM.Models;

namespace School_CRM.Models.ViewModels
{
    public class IdCardTemplateDto
    {
        public int TemplateId { get; set; }

        [Required(ErrorMessage = "Template Name is required")]
        [Display(Name = "Template Name")]
        public string TemplateName { get; set; } = null!;

        [Required]
        public string Orientation { get; set; } = "Vertical";

        [Display(Name = "School Name")]
        public string? SchoolName { get; set; }

        [Display(Name = "School Address")]
        public string? SchoolAddress { get; set; }

        [Display(Name = "School Contact")]
        public string? SchoolContact { get; set; }

        [Display(Name = "Theme Color (Hex)")]
        public string? ThemeColor { get; set; }

        public string? BackgroundFrontPath { get; set; }
        public string? BackgroundBackPath { get; set; }
        public string? SchoolLogoPath { get; set; }
        public string? PrincipalSignaturePath { get; set; }

        [Display(Name = "Fields Configuration (JSON)")]
        public string? FieldsConfigJson { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        // File uploads
        [Display(Name = "Front Background")]
        public IFormFile? BackgroundFrontFile { get; set; }

        [Display(Name = "Back Background")]
        public IFormFile? BackgroundBackFile { get; set; }

        [Display(Name = "School Logo")]
        public IFormFile? SchoolLogoFile { get; set; }

        [Display(Name = "Principal Signature")]
        public IFormFile? PrincipalSignatureFile { get; set; }
    }

    public class IdCardGenerateVM
    {
        public List<IdCardTemplateDto> Templates { get; set; } = new();
        public List<SelectListItemDto> Classes { get; set; } = new();
    }

    public class IdCardStudentDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string? RollNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
        public string? FatherName { get; set; }
        public string? Phone { get; set; }
        public string? PhotoPath { get; set; }
    }
}
