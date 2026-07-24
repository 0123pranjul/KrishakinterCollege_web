using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace School_CRM.Models.ViewModels
{
    public class NewAdmissionViewModel
    {
        // ── Student Basic Info ──────────────────────────────────────────────
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student name is required.")]
        [StringLength(100)]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = string.Empty;

        [Display(Name = "Admission No")]
        [StringLength(50)]
        public string? AdmissionNo { get; set; }

        [Display(Name = "Roll No")]
        [StringLength(20)]
        public string? RollNo { get; set; }

        [Required(ErrorMessage = "Admission date is required.")]
        [Display(Name = "Admission Date")]
        [DataType(DataType.Date)]
        public DateOnly AdmissionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Date of birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Blood Group")]
        [StringLength(5)]
        public string? BloodGroup { get; set; }

        [Display(Name = "Aadhaar No")]
        [StringLength(20)]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar must be 12 digits.")]
        public string? AadhaarNo { get; set; }

        [Display(Name = "Previous School")]
        [StringLength(200)]
        public string? PreviousSchool { get; set; }

        // ── Address ─────────────────────────────────────────────────────────
        [Display(Name = "Address Line 1")]
        [StringLength(200)]
        public string? AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Display(Name = "City")]
        [StringLength(50)]
        public string? City { get; set; }

        [Display(Name = "State")]
        [StringLength(50)]
        public string? State { get; set; }

        [Display(Name = "Pincode")]
        [StringLength(10)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits.")]
        public string? Pincode { get; set; }

        // ── Emergency Contact ────────────────────────────────────────────────
        [Display(Name = "Emergency Contact Name")]
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Number")]
        [StringLength(15)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string? EmergencyContactNumber { get; set; }

        // ── Photo ────────────────────────────────────────────────────────────
        [Display(Name = "Student Photo")]
        public IFormFile? PhotoFile { get; set; }

        public string? PhotoUrl { get; set; }

        // ── Parent / Guardian ────────────────────────────────────────────────
        [Required(ErrorMessage = "Father/Guardian name is required.")]
        [Display(Name = "Father / Guardian Name")]
        [StringLength(100)]
        public string FatherName { get; set; } = string.Empty;

        [Display(Name = "Father Mobile")]
        [StringLength(15)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string? FatherMobile { get; set; }

        [Display(Name = "Father Email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? FatherEmail { get; set; }

        [Display(Name = "Father Occupation")]
        [StringLength(100)]
        public string? FatherOccupation { get; set; }

        [Display(Name = "Mother Name")]
        [StringLength(100)]
        public string? MotherName { get; set; }

        [Display(Name = "Mother Mobile")]
        [StringLength(15)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string? MotherMobile { get; set; }

        [Display(Name = "Mother Occupation")]
        [StringLength(100)]
        public string? MotherOccupation { get; set; }

        // ── Session / Class / Section ────────────────────────────────────────
        [Required(ErrorMessage = "Academic session is required.")]
        [Display(Name = "Academic Session")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Class is required.")]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Section is required.")]
        [Display(Name = "Section")]
        public int SectionId { get; set; }

        // ── Dropdowns ────────────────────────────────────────────────────────
        public List<SelectListItem> Sessions { get; set; } = new();
        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Sections { get; set; } = new();
    }
}
