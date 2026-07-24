using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnFuelLog
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public DateOnly FuelDate { get; set; }

    public decimal FuelQuantityLitres { get; set; }

    public decimal FuelCostPerLitre { get; set; }

    public decimal? TotalFuelCost { get; set; }

    public int OdometerReading { get; set; }

    public string? FuelStation { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblTrnVehicle Vehicle { get; set; } = null!;
}
