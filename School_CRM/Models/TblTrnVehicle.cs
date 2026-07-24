using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnVehicle
{
    public int VehicleId { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public string VehicleType { get; set; } = null!;

    public string? Make { get; set; }

    public string? Model { get; set; }

    public short? ManufactureYear { get; set; }

    public short MaxCapacity { get; set; }

    public DateOnly? InsuranceExpiry { get; set; }

    public DateOnly? FitnessExpiry { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblTrnFuelLog> TblTrnFuelLogs { get; set; } = new List<TblTrnFuelLog>();

    public virtual ICollection<TblTrnMaintenanceLog> TblTrnMaintenanceLogs { get; set; } = new List<TblTrnMaintenanceLog>();

    public virtual ICollection<TblTrnVehicleAssignment> TblTrnVehicleAssignments { get; set; } = new List<TblTrnVehicleAssignment>();
}
