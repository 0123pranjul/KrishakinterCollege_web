using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string? EmployeeCode { get; set; }

    public string? Name { get; set; }

    public string? Designation { get; set; }

    public string? Department { get; set; }

    public decimal? BasicSalary { get; set; }

    public decimal? DailyRate { get; set; }

    public decimal? OvertimeRate { get; set; }

    public bool? LeaveWithoutPay { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<AttendanceMaster> AttendanceMasters { get; set; } = new List<AttendanceMaster>();

    public virtual ICollection<EmployeeAdvance> EmployeeAdvances { get; set; } = new List<EmployeeAdvance>();

    public virtual ICollection<EmployeeLeaf> EmployeeLeaves { get; set; } = new List<EmployeeLeaf>();

    public virtual ICollection<FaceEmbedding> FaceEmbeddings { get; set; } = new List<FaceEmbedding>();

    public virtual ICollection<SalaryMaster> SalaryMasters { get; set; } = new List<SalaryMaster>();

    public virtual ICollection<TblClassworkLog> TblClassworkLogs { get; set; } = new List<TblClassworkLog>();

    public virtual ICollection<TblHelpdeskTicket> TblHelpdeskTickets { get; set; } = new List<TblHelpdeskTicket>();

    public virtual ICollection<TblLessonPlan> TblLessonPlans { get; set; } = new List<TblLessonPlan>();

    public virtual ICollection<UserMaster> UserMasters { get; set; } = new List<UserMaster>();
}
