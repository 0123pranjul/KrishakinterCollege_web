using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class FaceEmbedding
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public byte[] Embedding { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
