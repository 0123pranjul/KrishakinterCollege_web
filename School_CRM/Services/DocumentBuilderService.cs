using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using School_CRM.Models;
using School_CRM.Models.ViewModels;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class DocumentBuilderService : IDocumentBuilderService
    {
        private readonly LibmanagementContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentBuilderService(LibmanagementContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        // ============================================================
        // TEMPLATES
        // ============================================================

        public async Task<List<TemplateListItemDto>> GetTemplatesAsync()
        {
            if (!await _context.DocBuilderTemplates.AnyAsync(t => t.IsActive))
            {
                await SeedDefaultTemplatesAsync();
            }

            return await _context.DocBuilderTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.TemplateName)
                .Select(t => new TemplateListItemDto
                {
                    TemplateId   = t.TemplateId,
                    TemplateName = t.TemplateName,
                    TemplateType = t.TemplateType,
                    Description  = t.Description,
                    ThumbnailUrl = t.ThumbnailUrl
                })
                .ToListAsync();
        }

        public async Task<DocBuilderTemplate?> GetTemplateByIdAsync(int id)
        {
            return await _context.DocBuilderTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == id && t.IsActive);
        }

        private async Task SeedDefaultTemplatesAsync()
        {
            var templates = new List<DocBuilderTemplate>
            {
                new DocBuilderTemplate
                {
                    TemplateName = "Exam Paper",
                    TemplateType = "ExamPaper",
                    Description = "Standard school examination paper template with header, details section, instructions, question block, and signatures.",
                    ThumbnailUrl = "",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    ComponentsJson = @"[
                        {
                            ""id"": ""comp_1"",
                            ""type"": ""logo"",
                            ""src"": ""/kic_logo.png"",
                            ""style"": {
                                ""width"": ""70px"",
                                ""height"": ""70px"",
                                ""display"": ""block"",
                                ""margin"": ""0 auto 8px auto""
                            },
                            ""order"": 0,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_2"",
                            ""type"": ""schoolName"",
                            ""content"": ""Krishak Inter College"",
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""22px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""center"",
                                ""color"": ""#1e293b"",
                                ""letterSpacing"": ""1px"",
                                ""marginBottom"": ""4px""
                            },
                            ""order"": 1,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_3"",
                            ""type"": ""paragraph"",
                            ""content"": ""G.T. Road, Ghatampur, Kanpur Nagar - 209206 | Affiliated to CBSE, New Delhi"",
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""12px"",
                                ""textAlign"": ""center"",
                                ""color"": ""#475569"",
                                ""marginBottom"": ""16px""
                            },
                            ""order"": 2,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_4"",
                            ""type"": ""title"",
                            ""content"": ""HALF YEARLY EXAMINATION 2025-26"",
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""16px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""center"",
                                ""color"": ""#0f172a"",
                                ""textDecoration"": ""underline"",
                                ""marginBottom"": ""16px""
                            },
                            ""order"": 3,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_5"",
                            ""type"": ""table"",
                            ""content"": ""<table class='table table-bordered' style='font-family: Times New Roman; font-size: 13px; margin-bottom: 12px;'><tbody><tr><td style='width:35%;'><strong>Student Name:</strong> ____________________</td><td style='width:35%;'><strong>Roll No:</strong> __________</td><td style='width:30%;'><strong>Section:</strong> ______</td></tr><tr><td><strong>Class:</strong> VIII</td><td><strong>Subject:</strong> Mathematics</td><td><strong>Date:</strong> __/__/____</td></tr><tr><td><strong>Time Allowed:</strong> 3 Hours</td><td><strong>Maximum Marks:</strong> 80</td><td><strong>Marks Obtained:</strong> _____</td></tr></tbody></table>"",
                            ""style"": {
                                ""marginBottom"": ""16px""
                            },
                            ""order"": 4,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_6"",
                            ""type"": ""instructions"",
                            ""content"": ""<strong>Instructions for Candidates:</strong><br/>1. All questions are compulsory. Reading time of 15 minutes is allowed.<br/>2. Section A contains 10 MCQs of 1 mark each.<br/>3. Section B contains 5 short answer questions of 2 marks each.<br/>4. Section C contains 5 long answer questions of 4 marks each.<br/>5. Use of calculator is strictly prohibited."",
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""12px"",
                                ""color"": ""#334155"",
                                ""padding"": ""8px 12px"",
                                ""backgroundColor"": ""#f8fafc"",
                                ""borderLeft"": ""3px solid #64748b"",
                                ""marginBottom"": ""20px""
                            },
                            ""order"": 5,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_7"",
                            ""type"": ""title"",
                            ""content"": ""SECTION A (Multiple Choice Questions)"",
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""14px"",
                                ""fontWeight"": ""bold"",
                                ""color"": ""#1e293b"",
                                ""marginBottom"": ""12px""
                            },
                            ""order"": 6,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_8"",
                            ""type"": ""mcq"",
                            ""questionNumber"": 1,
                            ""content"": ""What is the value of x if 3x - 5 = 10?"",
                            ""options"": [""x = 3"", ""x = 5"", ""x = 15"", ""x = 10""],
                            ""marks"": 1,
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""13px"",
                                ""marginBottom"": ""16px""
                            },
                            ""order"": 7,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_9"",
                            ""type"": ""mcq"",
                            ""questionNumber"": 2,
                            ""content"": ""The sum of interior angles of a pentagon is:"",
                            ""options"": [""360°"", ""540°"", ""180°"", ""720°""],
                            ""marks"": 1,
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""13px"",
                                ""marginBottom"": ""16px""
                            },
                            ""order"": 8,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_10"",
                            ""type"": ""pageBreak"",
                            ""style"": {},
                            ""order"": 9,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_11"",
                            ""type"": ""title"",
                            ""content"": ""SECTION B (Short Answer Questions)"",
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""14px"",
                                ""fontWeight"": ""bold"",
                                ""color"": ""#1e293b"",
                                ""marginBottom"": ""12px""
                            },
                            ""order"": 10,
                            ""page"": 2
                        },
                        {
                            ""id"": ""comp_12"",
                            ""type"": ""question"",
                            ""questionNumber"": 3,
                            ""content"": ""Define the Pythagorean Theorem and give one real-world application."",
                            ""marks"": 2,
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""13px"",
                                ""marginBottom"": ""40px""
                            },
                            ""order"": 11,
                            ""page"": 2
                        },
                        {
                            ""id"": ""comp_13"",
                            ""type"": ""question"",
                            ""questionNumber"": 4,
                            ""content"": ""Solve the quadratic equation x² - 5x + 6 = 0."",
                            ""marks"": 2,
                            ""style"": {
                                ""fontFamily"": ""Times New Roman"",
                                ""fontSize"": ""13px"",
                                ""marginBottom"": ""40px""
                            },
                            ""order"": 12,
                            ""page"": 2
                        },
                        {
                            ""id"": ""comp_14"",
                            ""type"": ""table"",
                            ""content"": ""<div class='d-flex justify-content-between' style='font-family: Times New Roman; font-size: 13px; margin-top: 100px; padding: 0 40px;'><div><br/><br/>_______________________<br/><strong>Teacher Signature</strong></div><div><br/><br/>_______________________<br/><strong>Principal Signature</strong></div></div>"",
                            ""style"": {},
                            ""order"": 13,
                            ""page"": 2
                        }
                    ]",
                    PrintSettingsJson = @"{
                        ""pageSize"": ""A4"",
                        ""orientation"": ""Portrait"",
                        ""marginTop"": 15.0,
                        ""marginBottom"": 15.0,
                        ""marginLeft"": 15.0,
                        ""marginRight"": 15.0,
                        ""showHeader"": false,
                        ""showFooter"": false,
                        ""showPageNumbers"": true
                    }"
                },
                new DocBuilderTemplate
                {
                    TemplateName = "Notice",
                    TemplateType = "Notice",
                    Description = "Standard school official Notice template with logo, header, date, subject, body, and signature block.",
                    ThumbnailUrl = "",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    ComponentsJson = @"[
                        {
                            ""id"": ""comp_1"",
                            ""type"": ""logo"",
                            ""src"": ""/kic_logo.png"",
                            ""style"": {
                                ""width"": ""80px"",
                                ""height"": ""80px"",
                                ""display"": ""block"",
                                ""margin"": ""0 auto 8px auto""
                            },
                            ""order"": 0,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_2"",
                            ""type"": ""schoolName"",
                            ""content"": ""Krishak Inter College"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""24px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""center"",
                                ""color"": ""#1e293b"",
                                ""marginBottom"": ""8px""
                            },
                            ""order"": 1,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_3"",
                            ""type"": ""title"",
                            ""content"": ""NOTICE"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""18px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""center"",
                                ""color"": ""#b91c1c"",
                                ""letterSpacing"": ""2px"",
                                ""textDecoration"": ""underline"",
                                ""marginBottom"": ""20px""
                            },
                            ""order"": 2,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_4"",
                            ""type"": ""table"",
                            ""content"": ""<table style='width:100%; font-family: Arial; font-size: 13px; margin-bottom: 20px;'><tbody><tr><td><strong>Ref No:</strong> GPS/2026/N-104</td><td style='text-align:right;'><strong>Date:</strong> 15th July 2026</td></tr></tbody></table>"",
                            ""style"": {},
                            ""order"": 3,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_5"",
                            ""type"": ""title"",
                            ""content"": ""Subject: Summer Vacation Holidays Announcement"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""14px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""left"",
                                ""color"": ""#0f172a"",
                                ""marginBottom"": ""16px""
                            },
                            ""order"": 4,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_6"",
                            ""type"": ""paragraph"",
                            ""content"": ""This is to inform all students, parents, and teachers that the school will remain closed for Summer Vacation from <strong>May 15, 2026</strong> to <strong>June 30, 2026</strong>. The school office will remain open for administrative works between 9:00 AM and 1:00 PM on all working days.<br/><br/>All students are advised to complete their Holiday Homework, which has been uploaded to the student portal. Extra remedial classes for Board students (Class X and XII) will be conducted online as per the schedule provided by their respective class teachers.<br/><br/>The school will reopen on <strong>July 1, 2026</strong> with normal school timings (7:30 AM to 1:30 PM). Happy Holidays!"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""13px"",
                                ""textAlign"": ""justify"",
                                ""color"": ""#334155"",
                                ""lineHeight"": ""1.6"",
                                ""marginBottom"": ""40px""
                            },
                            ""order"": 5,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_7"",
                            ""type"": ""signature"",
                            ""content"": ""<strong>(Principal)</strong><br/>Krishak Inter College"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""13px"",
                                ""textAlign"": ""left"",
                                ""marginTop"": ""80px""
                            },
                            ""order"": 6,
                            ""page"": 1
                        }
                    ]",
                    PrintSettingsJson = @"{
                        ""pageSize"": ""A4"",
                        ""orientation"": ""Portrait"",
                        ""marginTop"": 20.0,
                        ""marginBottom"": 20.0,
                        ""marginLeft"": 20.0,
                        ""marginRight"": 20.0,
                        ""showHeader"": false,
                        ""showFooter"": false,
                        ""showPageNumbers"": false
                    }"
                },
                new DocBuilderTemplate
                {
                    TemplateName = "Letter Head",
                    TemplateType = "LetterHead",
                    Description = "Standard school official Letter Head template with top header, side logo, and formal footer section.",
                    ThumbnailUrl = "",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    ComponentsJson = @"[
                        {
                            ""id"": ""comp_1"",
                            ""type"": ""table"",
                            ""content"": ""<table style='width:100%; border-bottom: 2px solid #1e3a8a; padding-bottom: 8px; margin-bottom: 20px;'><tbody><tr><td style='width:80px; vertical-align:middle;'><img src='/kic_logo.png' style='height:70px; width:70px;' /></td><td style='vertical-align:middle; padding-left:15px;'><h1 style='font-family:Arial; font-size:24px; font-weight:bold; color:#1e3a8a; margin:0;'>Krishak Inter College</h1><p style='font-family:Arial; font-size:11px; color:#475569; margin:2px 0 0 0;'>G.T. Road, Ghatampur, Kanpur Nagar - 209206 | Tel: +91 99351 06067 | Email: kic.otp@gmail.com</p></td></tr></tbody></table>"",
                            ""style"": {},
                            ""order"": 0,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_2"",
                            ""type"": ""table"",
                            ""content"": ""<table style='width:100%; font-family: Arial; font-size: 13px; margin-bottom: 40px;'><tbody><tr><td><strong>Ref No:</strong> GPS/LH/2026/________</td><td style='text-align:right;'><strong>Date:</strong> ____/____/2026</td></tr></tbody></table>"",
                            ""style"": {},
                            ""order"": 1,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_3"",
                            ""type"": ""paragraph"",
                            ""content"": ""To,<br/><strong>The Director,</strong><br/>Kanpur Educational Board,<br/>Civil Lines, Kanpur.<br/><br/><strong>Subject: __________________________________________________</strong><br/><br/>Dear Sir/Madam,<br/><br/>Write your letter content here... Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent eget purus sed erat aliquet finibus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Mauris facilisis erat eget turpis consequat, vel dapibus sapien tempus.<br/><br/>Proin vel erat dictum, lobortis eros in, elementum leo. Morbi egestas ex sed magna vulputate euismod. Duis viverra convallis ex sit amet auctor. Quisque a rhoncus urna. Ut et arcu in urna interdum convallis vitae at metus.<br/><br/>Thanking you.<br/><br/>Yours faithfully,"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""13px"",
                                ""color"": ""#334155"",
                                ""lineHeight"": ""1.6"",
                                ""marginBottom"": ""40px""
                            },
                            ""order"": 2,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_4"",
                            ""type"": ""signature"",
                            ""content"": ""<strong>Authorized Signatory</strong><br/>Krishak Inter College"",
                            ""style"": {
                                ""fontFamily"": ""Arial"",
                                ""fontSize"": ""13px"",
                                ""textAlign"": ""left"",
                                ""marginTop"": ""80px""
                            },
                            ""order"": 3,
                            ""page"": 1
                        }
                    ]",
                    PrintSettingsJson = @"{
                        ""pageSize"": ""A4"",
                        ""orientation"": ""Portrait"",
                        ""marginTop"": 20.0,
                        ""marginBottom"": 20.0,
                        ""marginLeft"": 20.0,
                        ""marginRight"": 20.0,
                        ""showHeader"": false,
                        ""showFooter"": false,
                        ""showPageNumbers"": false
                    }"
                },
                new DocBuilderTemplate
                {
                    TemplateName = "Certificate",
                    TemplateType = "Certificate",
                    Description = "Elegant school Certificate template with border, decorative text, date, and signature blocks.",
                    ThumbnailUrl = "",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    ComponentsJson = @"[
                        {
                            ""id"": ""comp_1"",
                            ""type"": ""logo"",
                            ""src"": ""/kic_logo.png"",
                            ""style"": {
                                ""width"": ""90px"",
                                ""height"": ""90px"",
                                ""display"": ""block"",
                                ""margin"": ""20px auto 10px auto""
                            },
                            ""order"": 0,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_2"",
                            ""type"": ""schoolName"",
                            ""content"": ""Krishak Inter College"",
                            ""style"": {
                                ""fontFamily"": ""Georgia"",
                                ""fontSize"": ""28px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""center"",
                                ""color"": ""#1e3a8a"",
                                ""letterSpacing"": ""2px"",
                                ""marginBottom"": ""4px""
                            },
                            ""order"": 1,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_3"",
                            ""type"": ""paragraph"",
                            ""content"": ""G.T. Road, Ghatampur, Kanpur Nagar - 209206"",
                            ""style"": {
                                ""fontFamily"": ""Georgia"",
                                ""fontSize"": ""12px"",
                                ""textAlign"": ""center"",
                                ""color"": ""#475569"",
                                ""marginBottom"": ""24px""
                            },
                            ""order"": 2,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_4"",
                            ""type"": ""title"",
                            ""content"": ""CERTIFICATE OF MERIT"",
                            ""style"": {
                                ""fontFamily"": ""Georgia"",
                                ""fontSize"": ""22px"",
                                ""fontWeight"": ""bold"",
                                ""textAlign"": ""center"",
                                ""color"": ""#b7791f"",
                                ""letterSpacing"": ""3px"",
                                ""marginBottom"": ""24px""
                            },
                            ""order"": 3,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_5"",
                            ""type"": ""paragraph"",
                            ""content"": ""This is to certify that<br/><h2 style='font-family:Georgia; font-size:24px; font-weight:bold; color:#0f172a; margin:16px 0;'>________________________________________</h2><br/>of Class <strong>________</strong> Section <strong>________</strong> has successfully secured <strong>________</strong> Position in the event <strong>________________________________________</strong> during the academic year <strong>2025-26</strong>.<br/><br/>His/Her performance in the activity was highly commendable."",
                            ""style"": {
                                ""fontFamily"": ""Georgia"",
                                ""fontSize"": ""14px"",
                                ""textAlign"": ""center"",
                                ""color"": ""#334155"",
                                ""lineHeight"": ""1.8"",
                                ""marginBottom"": ""40px""
                            },
                            ""order"": 4,
                            ""page"": 1
                        },
                        {
                            ""id"": ""comp_6"",
                            ""type"": ""table"",
                            ""content"": ""<table style='width:100%; font-family: Georgia; font-size: 13px; margin-top: 50px;'><tbody><tr><td style='text-align:center;'>___________________________<br/><strong>Date</strong></td><td style='text-align:center;'>___________________________<br/><strong>Class Teacher</strong></td><td style='text-align:center;'>___________________________<br/><strong>Principal</strong></td></tr></tbody></table>"",
                            ""style"": {},
                            ""order"": 5,
                            ""page"": 1
                        }
                    ]",
                    PrintSettingsJson = @"{
                        ""pageSize"": ""A4"",
                        ""orientation"": ""Landscape"",
                        ""marginTop"": 15.0,
                        ""marginBottom"": 15.0,
                        ""marginLeft"": 15.0,
                        ""marginRight"": 15.0,
                        ""showHeader"": false,
                        ""showFooter"": false,
                        ""showPageNumbers"": false
                    }"
                }
            };

            _context.DocBuilderTemplates.AddRange(templates);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // DOCUMENTS
        // ============================================================

        public async Task<List<DocumentListItemDto>> GetDocumentsAsync(int userId, string? filterType = null)
        {
            var query = _context.DocBuilderDocuments
                .Include(d => d.Template)
                .Where(d => d.CreatedBy == userId && d.IsActive);

            if (!string.IsNullOrWhiteSpace(filterType))
            {
                query = query.Where(d => d.DocumentType == filterType);
            }

            return await query
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .Select(d => new DocumentListItemDto
                {
                    DocumentId   = d.DocumentId,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    Status       = d.Status,
                    TemplateName = d.Template != null ? d.Template.TemplateName : null,
                    CreatedAt    = d.CreatedAt,
                    UpdatedAt    = d.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<DocBuilderDocument?> GetDocumentByIdAsync(int id)
        {
            return await _context.DocBuilderDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == id && d.IsActive);
        }

        // ============================================================
        // SAVE / UPDATE
        // ============================================================

        public async Task<int> SaveDocumentAsync(SaveDocumentDto dto, int userId)
        {
            int documentId = 0;

            if (dto.DocumentId.HasValue && dto.DocumentId.Value > 0)
            {
                // -- Update existing document --
                var doc = await _context.DocBuilderDocuments
                    .FirstOrDefaultAsync(d => d.DocumentId == dto.DocumentId.Value && d.IsActive);

                if (doc == null)
                    throw new KeyNotFoundException($"Document #{dto.DocumentId} not found.");

                doc.DocumentName    = dto.DocumentName;
                doc.DocumentType    = dto.DocumentType;
                doc.TemplateId      = dto.TemplateId;
                doc.ComponentsJson  = dto.ComponentsJson;
                doc.PrintSettingsJson = dto.PrintSettingsJson;
                doc.Status          = dto.Status;
                doc.UpdatedBy       = userId;
                doc.UpdatedAt       = DateTime.Now;

                await _context.SaveChangesAsync();
                documentId = doc.DocumentId;
            }
            else
            {
                // -- Create new document --
                var doc = new DocBuilderDocument
                {
                    DocumentName     = dto.DocumentName,
                    DocumentType     = dto.DocumentType,
                    TemplateId       = dto.TemplateId,
                    ComponentsJson   = dto.ComponentsJson,
                    PrintSettingsJson = dto.PrintSettingsJson,
                    Status           = dto.Status,
                    IsActive         = true,
                    CreatedBy        = userId,
                    CreatedAt        = DateTime.Now
                };

                _context.DocBuilderDocuments.Add(doc);
                await _context.SaveChangesAsync();
                documentId = doc.DocumentId;
            }

            // --- Phase 2: Extract Questions and sync to DocBuilder_Questions table ---
            try
            {
                // 1. Delete existing questions for this document
                var existingQuestions = await _context.DocBuilderQuestions
                    .Where(q => q.DocumentId == documentId)
                    .ToListAsync();
                
                if (existingQuestions.Any())
                {
                    _context.DocBuilderQuestions.RemoveRange(existingQuestions);
                    await _context.SaveChangesAsync();
                }

                // 2. Parse JSON and insert new questions
                if (!string.IsNullOrWhiteSpace(dto.ComponentsJson))
                {
                    using (JsonDocument jsonDoc = JsonDocument.Parse(dto.ComponentsJson))
                    {
                        var root = jsonDoc.RootElement;
                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var comp in root.EnumerateArray())
                            {
                                if (comp.TryGetProperty("type", out var typeProp))
                                {
                                    string type = typeProp.GetString() ?? "";
                                    if (type == "question" || type == "mcq" || type == "fillBlanks" || type == "trueFalse" || type == "matchFollowing")
                                    {
                                        var newQuestion = new DocBuilderQuestion
                                        {
                                            DocumentId = documentId,
                                            QuestionType = type,
                                            IsActive = true,
                                            CreatedAt = DateTime.Now,
                                            Marks = 1, // Default
                                            SortOrder = 0,
                                            QuestionText = ""
                                        };

                                        if (comp.TryGetProperty("content", out var contentProp))
                                            newQuestion.QuestionText = contentProp.GetString() ?? "";
                                            
                                        if (comp.TryGetProperty("order", out var orderProp))
                                            newQuestion.SortOrder = orderProp.GetInt32();

                                        if (comp.TryGetProperty("questionProps", out var props))
                                        {
                                            if (props.TryGetProperty("qNumber", out var qNumProp))
                                            {
                                                if (int.TryParse(qNumProp.GetString(), out int qNum))
                                                    newQuestion.QuestionNumber = qNum;
                                            }

                                            if (props.TryGetProperty("marks", out var marksProp))
                                            {
                                                if (decimal.TryParse(marksProp.GetString(), out decimal marks))
                                                    newQuestion.Marks = marks;
                                            }
                                                
                                            if (props.TryGetProperty("space", out var spaceProp))
                                            {
                                                if (int.TryParse(spaceProp.GetString(), out int space))
                                                    newQuestion.AnswerSpace = space;
                                            }
                                                
                                            if (type == "mcq" && props.TryGetProperty("options", out var optionsProp))
                                            {
                                                newQuestion.OptionsJson = optionsProp.GetRawText();
                                            }
                                        }

                                        _context.DocBuilderQuestions.Add(newQuestion);
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the document save
                Console.WriteLine($"Error extracting questions: {ex.Message}");
            }

            return documentId;
        }

        // ============================================================
        // SOFT DELETE
        // ============================================================

        public async Task<bool> DeleteDocumentAsync(int id, int userId)
        {
            var doc = await _context.DocBuilderDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == id && d.IsActive && d.CreatedBy == userId);

            if (doc == null)
                return false;

            doc.IsActive  = false;
            doc.UpdatedBy = userId;
            doc.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // IMAGE UPLOAD
        // ============================================================

        public async Task<DocBuilderImage> UploadImageAsync(IFormFile file, int? documentId, int userId)
        {
            var monthFolder = DateTime.Now.ToString("yyyy-MM");
            var relativePath = Path.Combine("uploads", "docbuilder", monthFolder);
            var absoluteDir  = Path.Combine(_env.WebRootPath, relativePath);

            if (!Directory.Exists(absoluteDir))
                Directory.CreateDirectory(absoluteDir);

            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var absolutePath = Path.Combine(absoluteDir, safeFileName);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var dbFilePath = $"/{relativePath.Replace("\\", "/")}/{safeFileName}";

            var image = new DocBuilderImage
            {
                DocumentId = documentId,
                FileName   = file.FileName,
                FilePath   = dbFilePath,
                FileSize   = file.Length,
                MimeType   = file.ContentType,
                IsActive   = true,
                CreatedBy  = userId,
                CreatedAt  = DateTime.Now
            };

            _context.DocBuilderImages.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }

        // ============================================================
        // QUESTION BANK
        // ============================================================

        public async Task<List<QuestionDto>> GetQuestionsAsync(string? filterType = null, int? classId = null, int? subjectId = null)
        {
            var query = _context.DocBuilderQuestions
                .Include(q => q.Document)
                .Where(q => q.IsActive);

            if (!string.IsNullOrEmpty(filterType))
            {
                query = query.Where(q => q.QuestionType == filterType);
            }
            if (classId.HasValue && classId.Value > 0)
            {
                query = query.Where(q => q.ClassId == classId.Value);
            }
            if (subjectId.HasValue && subjectId.Value > 0)
            {
                query = query.Where(q => q.SubjectId == subjectId.Value);
            }

            // Optional: Left join with TblClasses and TblSubjects for names
            var questions = await query
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionType = q.QuestionType,
                    QuestionText = q.QuestionText,
                    OptionsJson = q.OptionsJson,
                    Marks = q.Marks,
                    Difficulty = q.Difficulty,
                    AnswerSpace = q.AnswerSpace,
                    DocumentName = q.Document != null ? q.Document.DocumentName : "Manual Entry",
                    CreatedAt = q.CreatedAt,
                    ClassId = q.ClassId,
                    SubjectId = q.SubjectId,
                    ClassName = _context.TblClasses.Where(c => c.ClassId == q.ClassId).Select(c => c.ClassName).FirstOrDefault(),
                    SubjectName = _context.TblSubjects.Where(s => s.SubjectId == q.SubjectId).Select(s => s.SubjectName).FirstOrDefault()
                })
                .ToListAsync();
            
            return questions;
        }

        public async Task<int> SaveQuestionAsync(QuestionDto dto, int userId)
        {
            if (dto.QuestionId > 0)
            {
                var question = await _context.DocBuilderQuestions.FindAsync(dto.QuestionId);
                if (question == null || !question.IsActive)
                    throw new KeyNotFoundException("Question not found.");

                question.QuestionType = dto.QuestionType;
                question.QuestionText = dto.QuestionText;
                question.OptionsJson = dto.OptionsJson;
                question.Marks = dto.Marks;
                question.Difficulty = dto.Difficulty;
                question.AnswerSpace = dto.AnswerSpace;
                question.ClassId = dto.ClassId;
                question.SubjectId = dto.SubjectId;

                await _context.SaveChangesAsync();
                return question.QuestionId;
            }
            else
            {
                // Ensure a dummy document exists to satisfy the foreign key constraint
                var defaultDoc = await _context.DocBuilderDocuments
                    .FirstOrDefaultAsync(d => d.DocumentName == "Question Bank Default");

                if (defaultDoc == null)
                {
                    defaultDoc = new DocBuilderDocument
                    {
                        DocumentName = "Question Bank Default",
                        DocumentType = "Bank",
                        Status = "Published",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ComponentsJson = "[]",
                        PrintSettingsJson = "{}"
                    };
                    _context.DocBuilderDocuments.Add(defaultDoc);
                    await _context.SaveChangesAsync();
                }

                var question = new DocBuilderQuestion
                {
                    DocumentId = defaultDoc.DocumentId, // Use the generated ID for the dummy document
                    QuestionNumber = 0,
                    QuestionType = dto.QuestionType,
                    QuestionText = dto.QuestionText,
                    OptionsJson = dto.OptionsJson,
                    Marks = dto.Marks,
                    Difficulty = dto.Difficulty,
                    AnswerSpace = dto.AnswerSpace,
                    SortOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ClassId = dto.ClassId,
                    SubjectId = dto.SubjectId
                };

                _context.DocBuilderQuestions.Add(question);
                await _context.SaveChangesAsync();
                return question.QuestionId;
            }
        }

        public async Task<List<SelectListItemDto>> GetActiveClassesAsync()
        {
            return await _context.TblClasses
                .Where(c => c.IsActive == true)
                .Select(c => new SelectListItemDto { Id = c.ClassId, Name = c.ClassName ?? "Unknown Class" })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<SelectListItemDto>> GetActiveSubjectsAsync()
        {
            return await _context.TblSubjects
                .Where(s => s.IsActive == true)
                .Select(s => new SelectListItemDto { Id = s.SubjectId, Name = s.SubjectName })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            var question = await _context.DocBuilderQuestions.FindAsync(id);
            if (question == null)
                return false;

            question.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
