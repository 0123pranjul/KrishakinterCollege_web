using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentDocument
{
    public int DocumentId { get; set; }

    public int? StudentId { get; set; }

    public string? DocumentType { get; set; }

    public string? DocumentName { get; set; }

    public string? DocumentUrl { get; set; }

    public DateTime? UploadedDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblStudent? Student { get; set; }
}
