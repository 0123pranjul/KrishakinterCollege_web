using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.IO;
using System.Linq;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;

namespace School_CRM.Controllers
{
    public class BackupController : Controller
    {
        private readonly LibmanagementContext _context;
        private readonly IWebHostEnvironment _env;

        public BackupController(LibmanagementContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            string backupPath = Path.Combine(_env.WebRootPath, "backups");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            var files = Directory.GetFiles(backupPath, "*.bak")
                                 .Select(f => new FileInfo(f))
                                 .OrderByDescending(f => f.CreationTime)
                                 .ToList();

            return View(files);
        }

        [HttpPost]
        public IActionResult GenerateBackup()
        {
            try
            {
                string backupPath = Path.Combine(_env.WebRootPath, "backups");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                string dbName = _context.Database.GetDbConnection().Database;
                string fileName = $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                
                // IMPORTANT: The SQL Server must have write permissions to this fullPath
                string fullPath = Path.Combine(backupPath, fileName);

                string sql = $"BACKUP DATABASE [{dbName}] TO DISK = @path";
                var pathParam = new SqlParameter("@path", fullPath);
                
                // Setting command timeout longer as backup can take time
                _context.Database.SetCommandTimeout(300);
                _context.Database.ExecuteSqlRaw(sql, pathParam);

                TempData["Success"] = "Database Backup generated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error generating backup: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult Download(string fileName)
        {
            string backupPath = Path.Combine(_env.WebRootPath, "backups");
            string fullPath = Path.Combine(backupPath, fileName);

            if (System.IO.File.Exists(fullPath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                return File(fileBytes, "application/octet-stream", fileName);
            }

            TempData["Error"] = "Backup file not found on the server.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(string fileName)
        {
            try
            {
                string backupPath = Path.Combine(_env.WebRootPath, "backups");
                string fullPath = Path.Combine(backupPath, fileName);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    TempData["Success"] = "Backup file deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Backup file not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting file: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
