using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class LibraryMemberService : ILibraryMemberService
    {
        private readonly LibmanagementContext _context;

        public LibraryMemberService(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<List<MemberLookupDto>> GetStudentsAsync(string? search = null)
        {
            var query = _context.TblStudents
                .Where(s => s.IsActive == true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s =>
                    (s.StudentName != null && s.StudentName.ToLower().Contains(searchLower)) ||
                    (s.AdmissionNo != null && s.AdmissionNo.Contains(searchLower)) ||
                    (s.RollNo != null && s.RollNo.Contains(searchLower)));
            }

            return await query
                .OrderBy(s => s.StudentName)
                .Take(50)
                .Select(s => new MemberLookupDto
                {
                    UserId    = s.StudentId,
                    Name      = s.StudentName ?? $"Student #{s.StudentId}",
                    Code      = s.AdmissionNo ?? "",
                    Photo     = s.PhotoUrl,
                    UserType  = "Student"
                })
                .ToListAsync();
        }

        public async Task<List<MemberLookupDto>> GetTeachersAsync(string? search = null)
        {
            var query = _context.TblTeachers
                .Where(t => t.IsActive == true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(t =>
                    t.TeacherName.ToLower().Contains(searchLower) ||
                    (t.Email != null && t.Email.ToLower().Contains(searchLower)) ||
                    (t.MobileNo != null && t.MobileNo.Contains(searchLower)));
            }

            return await query
                .OrderBy(t => t.TeacherName)
                .Take(50)
                .Select(t => new MemberLookupDto
                {
                    UserId   = t.TeacherId,
                    Name     = t.TeacherName,
                    Code     = t.Email ?? "",
                    Photo    = null,
                    UserType = "Teacher"
                })
                .ToListAsync();
        }

        public async Task<MemberLookupDto?> GetMemberAsync(string userType, int userId)
        {
            if (userType == "Student")
            {
                var student = await _context.TblStudents
                    .Where(s => s.StudentId == userId)
                    .Select(s => new MemberLookupDto
                    {
                        UserId    = s.StudentId,
                        Name      = s.StudentName ?? $"Student #{s.StudentId}",
                        Code      = s.AdmissionNo ?? "",
                        Photo     = s.PhotoUrl,
                        UserType  = "Student"
                    })
                    .FirstOrDefaultAsync();
                return student;
            }
            else
            {
                var teacher = await _context.TblTeachers
                    .Where(t => t.TeacherId == userId)
                    .Select(t => new MemberLookupDto
                    {
                        UserId   = t.TeacherId,
                        Name     = t.TeacherName,
                        Code     = t.Email ?? "",
                        Photo    = null,
                        UserType = "Teacher"
                    })
                    .FirstOrDefaultAsync();
                return teacher;
            }
        }
    }
}
