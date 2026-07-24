using School_CRM.Models;

// ============================================================
//  Report ViewModels — used by SchoolReportController views
//  Namespace: School_CRM (matches @model directives in views)
// ============================================================
namespace School_CRM
{
    public class StudentAttendanceReportVm
    {
        public int? StudentId  { get; set; }
        public int? ClassId    { get; set; }
        public int? SectionId  { get; set; }
        public int? SessionId  { get; set; }
        public int  Month      { get; set; } = DateTime.Today.Month;
        public int  Year       { get; set; } = DateTime.Today.Year;
        public List<TblStudentAttendance> Records { get; set; } = new();
        public int     Present    { get; set; }
        public int     Absent     { get; set; }
        public int     Late       { get; set; }
        public int     Total      { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ClassAttendanceRowVm
    {
        public int    StudentId   { get; set; }
        public string StudentName { get; set; } = null!;
        public int    Present     { get; set; }
        public int    Absent      { get; set; }
        public int    Late        { get; set; }
        public int    Total       { get; set; }
        public decimal Percentage { get; set; }

        public string ColorClass => Percentage < 75   ? "table-danger"
                                  : Percentage <= 85  ? "table-warning"
                                  : "table-success";
    }

    public class ClassPerformanceVm
    {
        public int?   ExamId      { get; set; }
        public int?   ClassId     { get; set; }
        public int?   SectionId   { get; set; }
        public string ExamName    { get; set; } = "";
        public List<SubjectStatVm>  SubjectStats { get; set; } = new();
        public List<TopStudentVm>   TopStudents  { get; set; } = new();
    }

    public class SubjectStatVm
    {
        public string  SubjectName   { get; set; } = null!;
        public decimal Average       { get; set; }
        public decimal Highest       { get; set; }
        public decimal Lowest        { get; set; }
        public int     TotalStudents { get; set; }
    }

    public class TopStudentVm
    {
        public int     StudentId    { get; set; }
        public string  StudentName  { get; set; } = null!;
        public decimal TotalMarks   { get; set; }
    }

    public class PendingFeeRowVm
    {
        public int     StudentId    { get; set; }
        public string  StudentName  { get; set; } = null!;
        public decimal TotalDue     { get; set; }
        public int     OldestMonth  { get; set; }
        public int     OldestYear   { get; set; }
    }

    public class DailyFeeVm
    {
        public DateTime Date        { get; set; }
        public decimal  TotalAmount { get; set; }
        public decimal  Cash        { get; set; }
        public decimal  Online      { get; set; }
        public decimal  UPI         { get; set; }
        public int      Count       { get; set; }
    }

    public class NotiStatVm
    {
        public string  NotificationType { get; set; } = null!;
        public int     Total            { get; set; }
        public int     ReadCount        { get; set; }
        public decimal ReadRate         { get; set; }
    }
}
