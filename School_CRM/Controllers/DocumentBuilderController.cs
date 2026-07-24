using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.ViewModels;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    public class DocumentBuilderController : Controller
    {
        private readonly IDocumentBuilderService _service;

        public DocumentBuilderController(IDocumentBuilderService service)
        {
            _service = service;
        }

        private int GetUserId() => int.Parse(Request.Cookies["userId"] ?? "0");

        // ============================================================
        // BUILDER PAGE  (GET /DocumentBuilder/Index?id=5)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            var vm = new DocumentBuilderIndexVM
            {
                Templates = await _service.GetTemplatesAsync(),
                Classes = await _service.GetActiveClassesAsync(),
                Subjects = await _service.GetActiveSubjectsAsync()
            };

            if (id.HasValue)
            {
                var doc = await _service.GetDocumentByIdAsync(id.Value);
                if (doc != null)
                {
                    vm.DocumentId       = doc.DocumentId;
                    vm.DocumentName     = doc.DocumentName;
                    vm.DocumentType     = doc.DocumentType;
                    vm.ComponentsJson   = doc.ComponentsJson;
                    vm.PrintSettingsJson = doc.PrintSettingsJson;
                }
            }

            ViewData["Layout"] = "~/Views/Shared/_BuilderLayout.cshtml";
            return View(vm);
        }

        // ============================================================
        // MY DOCUMENTS LIST  (GET /DocumentBuilder/MyDocuments?type=QuestionPaper)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> MyDocuments(string? type)
        {
            var userId = GetUserId();
            var vm = new MyDocumentsVM
            {
                Documents  = await _service.GetDocumentsAsync(userId, type),
                FilterType = type
            };

            return View(vm);
        }

        // ============================================================
        // API — GET TEMPLATES LIST
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _service.GetTemplatesAsync();
            return Json(templates);
        }

        // ============================================================
        // API — GET SINGLE TEMPLATE (with ComponentsJson)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await _service.GetTemplateByIdAsync(id);
            if (template == null)
                return Json(new { success = false, message = "Template not found." });

            return Json(new
            {
                success           = true,
                templateId        = template.TemplateId,
                templateName      = template.TemplateName,
                templateType      = template.TemplateType,
                description       = template.Description,
                componentsJson    = template.ComponentsJson,
                printSettingsJson = template.PrintSettingsJson
            });
        }

        // ============================================================
        // API — SAVE / UPDATE DOCUMENT
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument([FromBody] SaveDocumentDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            try
            {
                var userId     = GetUserId();
                var documentId = await _service.SaveDocumentAsync(dto, userId);
                return Json(new { success = true, documentId, message = "Document saved successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // API — SOFT DELETE DOCUMENT
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId  = GetUserId();
            var deleted = await _service.DeleteDocumentAsync(id, userId);

            return deleted
                ? Json(new { success = true,  message = "Document deleted successfully." })
                : Json(new { success = false, message = "Document not found or access denied." });
        }

        // ============================================================
        // API — UPLOAD IMAGE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file, int? documentId)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file uploaded." });

            try
            {
                var userId = GetUserId();
                var image  = await _service.UploadImageAsync(file, documentId, userId);

                return Json(new { success = true, filePath = image.FilePath, imageId = image.ImageId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Upload failed: {ex.Message}" });
            }
        }

        // ============================================================
        // API — GET SINGLE DOCUMENT
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetDocument(int id)
        {
            var doc = await _service.GetDocumentByIdAsync(id);
            if (doc == null)
                return Json(new { success = false, message = "Document not found." });

            return Json(new
            {
                success           = true,
                documentId        = doc.DocumentId,
                documentName      = doc.DocumentName,
                documentType      = doc.DocumentType,
                templateId        = doc.TemplateId,
                componentsJson    = doc.ComponentsJson,
                printSettingsJson = doc.PrintSettingsJson,
                status            = doc.Status
            });
        }

        // ============================================================
        // QUESTION BANK
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> QuestionBank(string? filterType, int? classId, int? subjectId)
        {
            var vm = new QuestionBankVM
            {
                FilterType = filterType,
                FilterClassId = classId,
                FilterSubjectId = subjectId,
                Questions = await _service.GetQuestionsAsync(filterType, classId, subjectId),
                Classes = await _service.GetActiveClassesAsync(),
                Subjects = await _service.GetActiveSubjectsAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestionsApi(string? filterType, int? classId, int? subjectId)
        {
            var questions = await _service.GetQuestionsAsync(filterType, classId, subjectId);
            return Json(new { success = true, data = questions });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveQuestionApi([FromBody] QuestionDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            try
            {
                var userId = GetUserId();
                var questionId = await _service.SaveQuestionAsync(dto, userId);
                return Json(new { success = true, questionId, message = "Question saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestionApi(int id)
        {
            var deleted = await _service.DeleteQuestionAsync(id);
            return deleted
                ? Json(new { success = true, message = "Question deleted." })
                : Json(new { success = false, message = "Question not found." });
        }
    }
}
