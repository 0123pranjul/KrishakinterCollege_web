using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.ViewModels;
using School_CRM.Services.Interface;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace School_CRM.Controllers
{
    public class IdCardController : Controller
    {
        private readonly IIdCardService _service;
        private readonly IWebHostEnvironment _env;

        public IdCardController(IIdCardService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _service.GetAllTemplatesAsync();
            return View(templates);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                var template = await _service.GetTemplateByIdAsync(id.Value);
                if (template == null) return NotFound();
                return View(template);
            }
            return View(new IdCardTemplateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(IdCardTemplateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Handle file uploads
            if (dto.BackgroundFrontFile != null)
                dto.BackgroundFrontPath = await UploadFile(dto.BackgroundFrontFile, "backgrounds");
            if (dto.BackgroundBackFile != null)
                dto.BackgroundBackPath = await UploadFile(dto.BackgroundBackFile, "backgrounds");
            if (dto.SchoolLogoFile != null)
                dto.SchoolLogoPath = await UploadFile(dto.SchoolLogoFile, "logos");
            if (dto.PrincipalSignatureFile != null)
                dto.PrincipalSignaturePath = await UploadFile(dto.PrincipalSignatureFile, "signatures");

            await _service.SaveTemplateAsync(dto);
            TempData["SuccessMessage"] = "Template saved successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteTemplateAsync(id);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var vm = new IdCardGenerateVM
            {
                Templates = await _service.GetAllTemplatesAsync(),
                Classes = await _service.GetActiveClassesAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var sections = await _service.GetSectionsByClassAsync(classId);
            return Json(sections);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents(int? classId, int? sectionId, string? searchQuery)
        {
            var students = await _service.GetStudentsForIdCardAsync(classId, sectionId, searchQuery);
            return Json(new { success = true, data = students });
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplateDetails(int templateId)
        {
            var template = await _service.GetTemplateByIdAsync(templateId);
            return Json(new { success = template != null, data = template });
        }

        private async Task<string> UploadFile(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "idcards", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/idcards/{folder}/{uniqueFileName}";
        }
    }
}
