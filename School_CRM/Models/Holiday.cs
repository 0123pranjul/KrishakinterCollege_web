using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class Holiday
{
    public int Id { get; set; }

    public DateOnly? HolidayDate { get; set; }

    public string? HolidayName { get; set; }

    public string? MonthYear { get; set; }
}
