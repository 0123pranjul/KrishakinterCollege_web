using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnMaintenanceLog
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public string ServiceType { get; set; } = null!;

    public DateOnly ServiceDate { get; set; }

    public decimal ServiceCost { get; set; }

    public string? ServiceProvider { get; set; }

    public DateOnly? NextServiceDueDate { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblTrnVehicle Vehicle { get; set; } = null!;
}
