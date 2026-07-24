using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models;
using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "User name is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Captcha is required")]
    public string CaptchaInput { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Please select a role")]
    public int RoleId { get; set; }

    // Optional FK links — set based on role during registration
    public int? StudentId  { get; set; }
    public int? TeacherId  { get; set; }
    public int? EmpId      { get; set; }

    // Teacher creation properties
    public bool CreateTeacher { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherMobileNo { get; set; }
    public string? TeacherEmail { get; set; }
    public string? TeacherDesignation { get; set; }
    public DateOnly? TeacherJoiningDate { get; set; }

    // Employee creation properties
    public bool CreateEmployee { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeDesignation { get; set; }
    public string? EmployeeDepartment { get; set; }
    public decimal? EmployeeBasicSalary { get; set; }
}

public class UserLinkageDto
{
    public string Type { get; set; }  // "Student" / "Teacher" / "Employee"
    public string Name { get; set; }  // Name of the linked entity
}

public class UserListViewModel
{
    public int UserId       { get; set; }
    public string Username  { get; set; }
    public string Email     { get; set; }
    public string RoleName  { get; set; }
    public int    RoleId    { get; set; }
    public string LinkedTo  { get; set; }   // Student / Teacher / Employee name (fallback/first linkage)
    public string LinkedType { get; set; }  // "Student" / "Teacher" / "Employee" / "-" (fallback/first linkage)
    public List<UserLinkageDto> Linkages { get; set; } = new List<UserLinkageDto>();
    public bool   IsActive  { get; set; }
    public DateTime? CreatedDate { get; set; }
}
public class RoleViewModel
{
    [Required]
    [StringLength(50)]
    public string RoleName { get; set; }

    [StringLength(250)]
    public string Description { get; set; }

    public List<RoleMaster>? RoleList { get; set; }
}

public class AssignMenuPermissionViewModel
{
    public string SelectedRole { get; set; }
    public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    public List<MenuPermissionDto> MenuPermissions { get; set; } = new List<MenuPermissionDto>();
}
public class MenuPermissionDto
{
    public int MenuId { get; set; }
    public string MenuName { get; set; }
    public bool CanRead { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public List<MenuPermissionDto> Children { get; set; } = new List<MenuPermissionDto>();
}