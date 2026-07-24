namespace School_CRM.Models.ViewModels
{
    public class ScheduleViewModel
    {
        public int    TeacherId           { get; set; }
        public string TeacherName         { get; set; } = "";
        public string TeacherDesignation  { get; set; } = "";
        public int    SessionId           { get; set; }
        public int    TotalPeriodsWeek    { get; set; }
        public List<DayStatViewModel>   DayStats   { get; set; } = new();
        public List<PeriodRowViewModel> PeriodRows { get; set; } = new();
    }

    public class DayStatViewModel
    {
        public int    DayNumber  { get; set; }
        public string DayName    { get; set; } = "";
        public string DayShort   { get; set; } = "";
        public int    TotalSlots { get; set; }
    }

    public class PeriodRowViewModel
    {
        public int    PeriodId   { get; set; }
        public string PeriodName { get; set; } = "";
        public string StartTime  { get; set; } = "";
        public string EndTime    { get; set; } = "";
        public bool   IsBrake    { get; set; }
        public List<ScheduleCellViewModel> Cells { get; set; } = new();
    }

    public class ScheduleCellViewModel
    {
        public int    DayNumber   { get; set; }
        public bool   HasEntry    { get; set; }
        public string ClassName   { get; set; } = "";
        public string SectionName { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public int    TimeTableId { get; set; }
    }
}
