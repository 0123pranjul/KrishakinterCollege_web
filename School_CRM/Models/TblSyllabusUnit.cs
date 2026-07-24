using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblSyllabusUnit
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int SubjectId { get; set; }

    public string UnitName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;

    public virtual ICollection<TblSyllabusTopic> TblSyllabusTopics { get; set; } = new List<TblSyllabusTopic>();
}
