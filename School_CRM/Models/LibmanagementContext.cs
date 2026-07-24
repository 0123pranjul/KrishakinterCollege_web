using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace School_CRM.Models;

public partial class LibmanagementContext : DbContext
{
    public LibmanagementContext()
    {
    }

    public LibmanagementContext(DbContextOptions<LibmanagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsmAsset> AsmAssets { get; set; }

    public virtual DbSet<AsmAssetUnit> AsmAssetUnits { get; set; }

    public virtual DbSet<AsmCategory> AsmCategories { get; set; }

    public virtual DbSet<AsmDamageLossReport> AsmDamageLossReports { get; set; }

    public virtual DbSet<AsmDisposalLog> AsmDisposalLogs { get; set; }

    public virtual DbSet<AsmIssueTransaction> AsmIssueTransactions { get; set; }

    public virtual DbSet<AsmLocation> AsmLocations { get; set; }

    public virtual DbSet<AsmLocationHistory> AsmLocationHistories { get; set; }

    public virtual DbSet<AsmMaintenanceLog> AsmMaintenanceLogs { get; set; }

    public virtual DbSet<AsmSubCategory> AsmSubCategories { get; set; }

    public virtual DbSet<AsmVendor> AsmVendors { get; set; }

    public virtual DbSet<AttendanceMaster> AttendanceMasters { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CommAnnouncement> CommAnnouncements { get; set; }

    public virtual DbSet<CommAnnouncementRead> CommAnnouncementReads { get; set; }

    public virtual DbSet<CommCircular> CommCirculars { get; set; }

    public virtual DbSet<CommEvent> CommEvents { get; set; }

    public virtual DbSet<CommMessage> CommMessages { get; set; }

    public virtual DbSet<CommMessageThread> CommMessageThreads { get; set; }

    public virtual DbSet<CommNotification> CommNotifications { get; set; }

    public virtual DbSet<CommNotificationTemplate> CommNotificationTemplates { get; set; }

    public virtual DbSet<CommScheduledJob> CommScheduledJobs { get; set; }

    public virtual DbSet<DocBuilderDocument> DocBuilderDocuments { get; set; }

    public virtual DbSet<DocBuilderImage> DocBuilderImages { get; set; }

    public virtual DbSet<DocBuilderPrintSetting> DocBuilderPrintSettings { get; set; }

    public virtual DbSet<DocBuilderQuestion> DocBuilderQuestions { get; set; }

    public virtual DbSet<DocBuilderTemplate> DocBuilderTemplates { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeAdvance> EmployeeAdvances { get; set; }

    public virtual DbSet<EmployeeLeaf> EmployeeLeaves { get; set; }

    public virtual DbSet<FaceEmbedding> FaceEmbeddings { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<InvCategory> InvCategories { get; set; }

    public virtual DbSet<InvCreditLedger> InvCreditLedgers { get; set; }

    public virtual DbSet<InvProduct> InvProducts { get; set; }

    public virtual DbSet<InvPurchaseOrder> InvPurchaseOrders { get; set; }

    public virtual DbSet<InvPurchaseOrderItem> InvPurchaseOrderItems { get; set; }

    public virtual DbSet<InvSaleItem> InvSaleItems { get; set; }

    public virtual DbSet<InvSaleTransaction> InvSaleTransactions { get; set; }

    public virtual DbSet<InvStockAdjustment> InvStockAdjustments { get; set; }

    public virtual DbSet<InvStockReceipt> InvStockReceipts { get; set; }

    public virtual DbSet<InvStockReceiptItem> InvStockReceiptItems { get; set; }

    public virtual DbSet<InvSupplier> InvSuppliers { get; set; }

    public virtual DbSet<InvUnit> InvUnits { get; set; }

    public virtual DbSet<LibBook> LibBooks { get; set; }

    public virtual DbSet<LibBookCategory> LibBookCategories { get; set; }

    public virtual DbSet<LibBookCopy> LibBookCopies { get; set; }

    public virtual DbSet<LibFinePayment> LibFinePayments { get; set; }

    public virtual DbSet<LibFinePolicy> LibFinePolicies { get; set; }

    public virtual DbSet<LibIssueTransaction> LibIssueTransactions { get; set; }

    public virtual DbSet<LibMemberBlockLog> LibMemberBlockLogs { get; set; }

    public virtual DbSet<LibSetting> LibSettings { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RoleMaster> RoleMasters { get; set; }

    public virtual DbSet<SalaryMaster> SalaryMasters { get; set; }

    public virtual DbSet<TblAcademicSession> TblAcademicSessions { get; set; }

    public virtual DbSet<TblAdmission> TblAdmissions { get; set; }

    public virtual DbSet<TblAnnouncement> TblAnnouncements { get; set; }

    public virtual DbSet<TblAssignment> TblAssignments { get; set; }

    public virtual DbSet<TblClass> TblClasses { get; set; }

    public virtual DbSet<TblClassSection> TblClassSections { get; set; }

    public virtual DbSet<TblClassSubject> TblClassSubjects { get; set; }

    public virtual DbSet<TblClassworkLog> TblClassworkLogs { get; set; }

    public virtual DbSet<TblContactQuery> TblContactQueries { get; set; }

    public virtual DbSet<TblCustomTest> TblCustomTests { get; set; }

    public virtual DbSet<TblCustomTestMark> TblCustomTestMarks { get; set; }

    public virtual DbSet<TblEnquiry> TblEnquiries { get; set; }

    public virtual DbSet<TblEnquiryDocument> TblEnquiryDocuments { get; set; }

    public virtual DbSet<TblEnquiryFollowUp> TblEnquiryFollowUps { get; set; }

    public virtual DbSet<TblExam> TblExams { get; set; }

    public virtual DbSet<TblExamMark> TblExamMarks { get; set; }

    public virtual DbSet<TblExamSubject> TblExamSubjects { get; set; }

    public virtual DbSet<TblExamWeightage> TblExamWeightages { get; set; }

    public virtual DbSet<TblFeeCollection> TblFeeCollections { get; set; }

    public virtual DbSet<TblFeeCollectionDetail> TblFeeCollectionDetails { get; set; }

    public virtual DbSet<TblFeeStructure> TblFeeStructures { get; set; }

    public virtual DbSet<TblFeeTransaction> TblFeeTransactions { get; set; }

    public virtual DbSet<TblFeeType> TblFeeTypes { get; set; }

    public virtual DbSet<TblGradeMaster> TblGradeMasters { get; set; }

    public virtual DbSet<TblHelpdeskCategory> TblHelpdeskCategories { get; set; }

    public virtual DbSet<TblHelpdeskReply> TblHelpdeskReplies { get; set; }

    public virtual DbSet<TblHelpdeskTicket> TblHelpdeskTickets { get; set; }

    public virtual DbSet<TblIdCardTemplate> TblIdCardTemplates { get; set; }

    public virtual DbSet<TblLessonCoverage> TblLessonCoverages { get; set; }

    public virtual DbSet<TblLessonPlan> TblLessonPlans { get; set; }

    public virtual DbSet<TblMenu> TblMenus { get; set; }

    public virtual DbSet<TblMenuPermission> TblMenuPermissions { get; set; }

    public virtual DbSet<TblPeriod> TblPeriods { get; set; }

    public virtual DbSet<TblPromotionLog> TblPromotionLogs { get; set; }

    public virtual DbSet<TblReportCard> TblReportCards { get; set; }

    public virtual DbSet<TblReportCardSubject> TblReportCardSubjects { get; set; }

    public virtual DbSet<TblSection> TblSections { get; set; }

    public virtual DbSet<TblStudent> TblStudents { get; set; }

    public virtual DbSet<TblStudentAttendance> TblStudentAttendances { get; set; }

    public virtual DbSet<TblStudentDocument> TblStudentDocuments { get; set; }

    public virtual DbSet<TblStudentDue> TblStudentDues { get; set; }

    public virtual DbSet<TblStudentExit> TblStudentExits { get; set; }

    public virtual DbSet<TblStudentExtraCharge> TblStudentExtraCharges { get; set; }

    public virtual DbSet<TblStudentFeeOverride> TblStudentFeeOverrides { get; set; }

    public virtual DbSet<TblStudentMedical> TblStudentMedicals { get; set; }

    public virtual DbSet<TblStudentOptionalFee> TblStudentOptionalFees { get; set; }

    public virtual DbSet<TblStudentParent> TblStudentParents { get; set; }

    public virtual DbSet<TblStudentSession> TblStudentSessions { get; set; }

    public virtual DbSet<TblStudyMaterial> TblStudyMaterials { get; set; }

    public virtual DbSet<TblSubject> TblSubjects { get; set; }

    public virtual DbSet<TblSyllabusTopic> TblSyllabusTopics { get; set; }

    public virtual DbSet<TblSyllabusUnit> TblSyllabusUnits { get; set; }

    public virtual DbSet<TblTeacher> TblTeachers { get; set; }

    public virtual DbSet<TblTeacherAssignment> TblTeacherAssignments { get; set; }

    public virtual DbSet<TblTimeTable> TblTimeTables { get; set; }

    public virtual DbSet<TblTrnConductor> TblTrnConductors { get; set; }

    public virtual DbSet<TblTrnDriver> TblTrnDrivers { get; set; }

    public virtual DbSet<TblTrnFuelLog> TblTrnFuelLogs { get; set; }

    public virtual DbSet<TblTrnGpsUpdate> TblTrnGpsUpdates { get; set; }

    public virtual DbSet<TblTrnMaintenanceLog> TblTrnMaintenanceLogs { get; set; }

    public virtual DbSet<TblTrnNotificationLog> TblTrnNotificationLogs { get; set; }

    public virtual DbSet<TblTrnRoute> TblTrnRoutes { get; set; }

    public virtual DbSet<TblTrnRouteStop> TblTrnRouteStops { get; set; }

    public virtual DbSet<TblTrnSetting> TblTrnSettings { get; set; }

    public virtual DbSet<TblTrnStudentAssignment> TblTrnStudentAssignments { get; set; }

    public virtual DbSet<TblTrnTrip> TblTrnTrips { get; set; }

    public virtual DbSet<TblTrnTripBoardingLog> TblTrnTripBoardingLogs { get; set; }

    public virtual DbSet<TblTrnVehicle> TblTrnVehicles { get; set; }

    public virtual DbSet<TblTrnVehicleAssignment> TblTrnVehicleAssignments { get; set; }

    public virtual DbSet<TblTrnWeeklySchedule> TblTrnWeeklySchedules { get; set; }

    public virtual DbSet<UserMaster> UserMasters { get; set; }

    public virtual DbSet<UserOtp> UserOtps { get; set; }

    public virtual DbSet<UserRoleAssign> UserRoleAssigns { get; set; }

    public virtual DbSet<VwLibBookStock> VwLibBookStocks { get; set; }

    public virtual DbSet<VwLibCurrentIssue> VwLibCurrentIssues { get; set; }

    public virtual DbSet<VwLibMemberBlockStatus> VwLibMemberBlockStatuses { get; set; }

   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsmAsset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__AsmAsset__43492352911B1D11");

            entity.HasIndex(e => e.AssetCode, "UQ__AsmAsset__2DDE5240BAA7885B").IsUnique();

            entity.Property(e => e.AssetCode).HasMaxLength(50);
            entity.Property(e => e.AssetImagePath).HasMaxLength(500);
            entity.Property(e => e.AssetName).HasMaxLength(200);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsIssuable).HasDefaultValue(true);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Specifications).HasMaxLength(1000);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.AsmAssets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmAssets__Categ__0AF29B96");

            entity.HasOne(d => d.SubCategory).WithMany(p => p.AsmAssets)
                .HasForeignKey(d => d.SubCategoryId)
                .HasConstraintName("FK__AsmAssets__SubCa__0BE6BFCF");
        });

        modelBuilder.Entity<AsmAssetUnit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PK__AsmAsset__44F5ECB58646F015");

            entity.HasIndex(e => e.AssetTag, "UQ__AsmAsset__89F276ABE4EC45D7").IsUnique();

            entity.Property(e => e.Amcexpiry).HasColumnName("AMCExpiry");
            entity.Property(e => e.Amcvendor)
                .HasMaxLength(200)
                .HasColumnName("AMCVendor");
            entity.Property(e => e.AssetTag).HasMaxLength(50);
            entity.Property(e => e.AssignedToType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(1000)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.QrcodeImagePath)
                .HasMaxLength(500)
                .HasColumnName("QRCodeImagePath");
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.UnitCondition)
                .HasMaxLength(20)
                .HasDefaultValue("Good");

            entity.HasOne(d => d.Asset).WithMany(p => p.AsmAssetUnits)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmAssetU__Asset__0CDAE408");

            entity.HasOne(d => d.CurrentLocation).WithMany(p => p.AsmAssetUnits)
                .HasForeignKey(d => d.CurrentLocationId)
                .HasConstraintName("FK__AsmAssetU__Curre__0DCF0841");

            entity.HasOne(d => d.Vendor).WithMany(p => p.AsmAssetUnits)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__AsmAssetU__Vendo__0EC32C7A");
        });

        modelBuilder.Entity<AsmCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__AsmCateg__19093A0BF94D0393");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<AsmDamageLossReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__AsmDamag__D5BD48057C35287A");

            entity.ToTable("AsmDamageLossReport");

            entity.Property(e => e.ActionTaken).HasMaxLength(300);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EstimatedLoss).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FineImposed).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.ReportDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.ReportType).HasMaxLength(20);
            entity.Property(e => e.ResponsibleType).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Open");

            entity.HasOne(d => d.Issue).WithMany(p => p.AsmDamageLossReports)
                .HasForeignKey(d => d.IssueId)
                .HasConstraintName("FK__AsmDamage__Issue__0FB750B3");

            entity.HasOne(d => d.Unit).WithMany(p => p.AsmDamageLossReports)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmDamage__UnitI__10AB74EC");
        });

        modelBuilder.Entity<AsmDisposalLog>(entity =>
        {
            entity.HasKey(e => e.DisposalId).HasName("PK__AsmDispo__206044230904F6A1");

            entity.ToTable("AsmDisposalLog");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisposalType).HasMaxLength(30);
            entity.Property(e => e.DisposedTo).HasMaxLength(200);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.SaleValue).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Unit).WithMany(p => p.AsmDisposalLogs)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmDispos__UnitI__119F9925");
        });

        modelBuilder.Entity<AsmIssueTransaction>(entity =>
        {
            entity.HasKey(e => e.IssueId).HasName("PK__AsmIssue__6C8616040DE3766D");

            entity.Property(e => e.ConditionOnIssue)
                .HasMaxLength(20)
                .HasDefaultValue("Good");
            entity.Property(e => e.ConditionOnReturn).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DamageFine).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.IssuedToType).HasMaxLength(20);
            entity.Property(e => e.Purpose).HasMaxLength(300);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.TransactionStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Issued");

            entity.HasOne(d => d.Unit).WithMany(p => p.AsmIssueTransactions)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmIssueT__UnitI__1293BD5E");
        });

        modelBuilder.Entity<AsmLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__AsmLocat__E7FEA4976658F0FD");

            entity.Property(e => e.Building).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Floor).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LocationName).HasMaxLength(150);
            entity.Property(e => e.LocationType).HasMaxLength(50);
        });

        modelBuilder.Entity<AsmLocationHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__AsmLocat__4D7B4ABDC8F12A1A");

            entity.ToTable("AsmLocationHistory");

            entity.Property(e => e.MoveDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(300);

            entity.HasOne(d => d.FromLocation).WithMany(p => p.AsmLocationHistoryFromLocations)
                .HasForeignKey(d => d.FromLocationId)
                .HasConstraintName("FK__AsmLocati__FromL__1387E197");

            entity.HasOne(d => d.ToLocation).WithMany(p => p.AsmLocationHistoryToLocations)
                .HasForeignKey(d => d.ToLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmLocati__ToLoc__147C05D0");

            entity.HasOne(d => d.Unit).WithMany(p => p.AsmLocationHistories)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmLocati__UnitI__15702A09");
        });

        modelBuilder.Entity<AsmMaintenanceLog>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId).HasName("PK__AsmMaint__E60542D588D30211");

            entity.ToTable("AsmMaintenanceLog");

            entity.Property(e => e.BillNo).HasMaxLength(100);
            entity.Property(e => e.ConditionAfter).HasMaxLength(20);
            entity.Property(e => e.ConditionBefore).HasMaxLength(20);
            entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MaintenanceType).HasMaxLength(30);
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.ServicedBy).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Unit).WithMany(p => p.AsmMaintenanceLogs)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmMainte__UnitI__16644E42");

            entity.HasOne(d => d.Vendor).WithMany(p => p.AsmMaintenanceLogs)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK__AsmMainte__Vendo__1758727B");
        });

        modelBuilder.Entity<AsmSubCategory>(entity =>
        {
            entity.HasKey(e => e.SubCategoryId).HasName("PK__AsmSubCa__26BE5B19DC3863A0");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SubCategoryName).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.AsmSubCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsmSubCat__Categ__184C96B4");
        });

        modelBuilder.Entity<AsmVendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__AsmVendo__FC8618F32BCB4BD4");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gstno)
                .HasMaxLength(20)
                .HasColumnName("GSTNo");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.VendorName).HasMaxLength(200);
        });

        modelBuilder.Entity<AttendanceMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07F3D98DE3");

            entity.ToTable("AttendanceMaster");

            entity.Property(e => e.HoursWorked)
                .HasDefaultValue(8m)
                .HasColumnType("decimal(4, 2)");
            entity.Property(e => e.OvertimeHours)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceMasters)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Attendanc__Emplo__1940BAED");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);

            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.ActionName).HasMaxLength(100);
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ChangedByName).HasMaxLength(100);
            entity.Property(e => e.ControllerName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.RecordId).HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.RequestUrl).HasMaxLength(500);
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.UserRole).HasMaxLength(50);
        });

        modelBuilder.Entity<CommAnnouncement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__CommAnno__9DE44574D1EBC90D");

            entity.Property(e => e.AttachmentName).HasMaxLength(200);
            entity.Property(e => e.AttachmentPath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.Priority)
                .HasMaxLength(10)
                .HasDefaultValue("Normal");
            entity.Property(e => e.PublishAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TargetType)
                .HasMaxLength(20)
                .HasDefaultValue("All");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CommAnnouncementRead>(entity =>
        {
            entity.HasKey(e => e.ReadId).HasName("PK__CommAnno__1FABC86C625B0FE3");

            entity.Property(e => e.ReadAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReaderType).HasMaxLength(20);

            entity.HasOne(d => d.Announcement).WithMany(p => p.CommAnnouncementReads)
                .HasForeignKey(d => d.AnnouncementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommAnnou__Annou__1A34DF26");
        });

        modelBuilder.Entity<CommCircular>(entity =>
        {
            entity.HasKey(e => e.CircularId).HasName("PK__CommCirc__C019C86ED4D4D671");

            entity.HasIndex(e => e.CircularNo, "UQ__CommCirc__C019E0FCCB6C75E3").IsUnique();

            entity.Property(e => e.CircularDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.CircularNo).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FileName).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileSizeKb).HasColumnName("FileSizeKB");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TargetType)
                .HasMaxLength(20)
                .HasDefaultValue("All");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<CommEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__CommEven__7944C810F444DE75");

            entity.Property(e => e.Color)
                .HasMaxLength(10)
                .HasDefaultValue("#3498db");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EventTitle).HasMaxLength(200);
            entity.Property(e => e.EventType).HasMaxLength(20);
            entity.Property(e => e.IsFullDay).HasDefaultValue(true);
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.TargetType)
                .HasMaxLength(20)
                .HasDefaultValue("All");
            entity.Property(e => e.Venue).HasMaxLength(200);
        });

        modelBuilder.Entity<CommMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__CommMess__C87C0C9C4DF52BC5");

            entity.Property(e => e.AttachmentPath).HasMaxLength(500);
            entity.Property(e => e.ReadAt).HasColumnType("datetime");
            entity.Property(e => e.SenderType).HasMaxLength(20);
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Thread).WithMany(p => p.CommMessages)
                .HasForeignKey(d => d.ThreadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommMessa__Threa__1B29035F");
        });

        modelBuilder.Entity<CommMessageThread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__CommMess__6883568467997F53");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InitiatorType).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastMessageAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecipientType).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<CommNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__CommNoti__20CF2E1212D61E75");

            entity.Property(e => e.Body).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NotificationType).HasMaxLength(30);
            entity.Property(e => e.Priority)
                .HasMaxLength(10)
                .HasDefaultValue("Normal");
            entity.Property(e => e.ReadAt).HasColumnType("datetime");
            entity.Property(e => e.RecipientType).HasMaxLength(20);
            entity.Property(e => e.RedirectUrl).HasMaxLength(300);
            entity.Property(e => e.ReferenceType).HasMaxLength(50);
            entity.Property(e => e.SendSms).HasColumnName("SendSMS");
            entity.Property(e => e.Smssent).HasColumnName("SMSSent");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<CommNotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__CommNoti__F87ADD273335BD7A");

            entity.Property(e => e.BodyTemplate).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NotificationType).HasMaxLength(30);
            entity.Property(e => e.TemplateName).HasMaxLength(100);
            entity.Property(e => e.TitleTemplate).HasMaxLength(200);
        });

        modelBuilder.Entity<CommScheduledJob>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__CommSche__056690C24E2BEFAA");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JobName).HasMaxLength(100);
            entity.Property(e => e.LastRunAt).HasColumnType("datetime");
            entity.Property(e => e.NextRunAt).HasColumnType("datetime");
            entity.Property(e => e.RunTime).HasDefaultValue(new TimeOnly(9, 0, 0));
            entity.Property(e => e.ScheduleType).HasMaxLength(10);
            entity.Property(e => e.TargetType)
                .HasMaxLength(20)
                .HasDefaultValue("All");

            entity.HasOne(d => d.Template).WithMany(p => p.CommScheduledJobs)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommSched__Templ__1C1D2798");
        });

        modelBuilder.Entity<DocBuilderDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__DocBuild__1ABEEF0FC8BA5C4D");

            entity.ToTable("DocBuilder_Documents");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentName).HasMaxLength(300);
            entity.Property(e => e.DocumentType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Template).WithMany(p => p.DocBuilderDocuments)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK__DocBuilde__Templ__4C8B54C9");
        });

        modelBuilder.Entity<DocBuilderImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__DocBuild__7516F70C22CD8999");

            entity.ToTable("DocBuilder_Images");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(300);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MimeType).HasMaxLength(100);

            entity.HasOne(d => d.Document).WithMany(p => p.DocBuilderImages)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK__DocBuilde__Docum__52442E1F");
        });

        modelBuilder.Entity<DocBuilderPrintSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__DocBuild__54372B1D428618E3");

            entity.ToTable("DocBuilder_PrintSettings");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FooterText).HasMaxLength(500);
            entity.Property(e => e.HeaderText).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MarginBottom)
                .HasDefaultValue(15m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MarginLeft)
                .HasDefaultValue(15m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MarginRight)
                .HasDefaultValue(15m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MarginTop)
                .HasDefaultValue(15m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Orientation)
                .HasMaxLength(20)
                .HasDefaultValue("Portrait");
            entity.Property(e => e.PageSize)
                .HasMaxLength(20)
                .HasDefaultValue("A4");
            entity.Property(e => e.SettingName).HasMaxLength(200);
            entity.Property(e => e.ShowPageNumbers).HasDefaultValue(true);
            entity.Property(e => e.WatermarkOpacity)
                .HasDefaultValue(0.10m)
                .HasColumnType("decimal(3, 2)");
            entity.Property(e => e.WatermarkText).HasMaxLength(200);
        });

        modelBuilder.Entity<DocBuilderQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__DocBuild__0DC06FACA43986B9");

            entity.ToTable("DocBuilder_Questions");

            entity.Property(e => e.AnswerSpace).HasDefaultValue(4);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Difficulty).HasMaxLength(20);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Marks)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(5, 1)");
            entity.Property(e => e.QuestionType).HasMaxLength(50);

            entity.HasOne(d => d.Document).WithMany(p => p.DocBuilderQuestions)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DocBuilde__Docum__5708E33C");
        });

        modelBuilder.Entity<DocBuilderTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__DocBuild__F87ADD27E532B8F9");

            entity.ToTable("DocBuilder_Templates");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsSystem).HasDefaultValue(true);
            entity.Property(e => e.TemplateName).HasMaxLength(200);
            entity.Property(e => e.TemplateType).HasMaxLength(50);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC074B538329");

            entity.HasIndex(e => e.EmployeeCode, "UQ__Employee__1F642548007F62F9").IsUnique();

            entity.Property(e => e.BasicSalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DailyRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Department).HasMaxLength(50);
            entity.Property(e => e.Designation).HasMaxLength(50);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LeaveWithoutPay).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.OvertimeRate).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<EmployeeAdvance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC0779F94C66");

            entity.ToTable("EmployeeAdvance");

            entity.Property(e => e.AdvanceDate).HasColumnType("datetime");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeductFromMonth)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.Reason).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeAdvances)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmpAdvanc__Emplo__Adv01");
        });

        modelBuilder.Entity<EmployeeLeaf>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC071148077D");

            entity.Property(e => e.LeaveType).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Approved");
            entity.Property(e => e.TotalDays).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeLeaves)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeL__Emplo__1E05700A");
        });

        modelBuilder.Entity<FaceEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FaceEmbe__3214EC07537E715A");

            entity.HasOne(d => d.Employee).WithMany(p => p.FaceEmbeddings)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FaceEmbeddings_Employee");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Holidays__3214EC07A594F385");

            entity.Property(e => e.HolidayName).HasMaxLength(100);
            entity.Property(e => e.MonthYear).HasMaxLength(7);
        });

        modelBuilder.Entity<InvCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__InvCateg__19093A0BD3DE1BB7");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<InvCreditLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerId).HasName("PK__InvCredi__AE70E0CFB035F48A");

            entity.ToTable("InvCreditLedger");

            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerType).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.TransactionType).HasMaxLength(10);

            entity.HasOne(d => d.Sale).WithMany(p => p.InvCreditLedgers)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("FK__InvCredit__SaleI__1FEDB87C");
        });

        modelBuilder.Entity<InvProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__InvProdu__B40CC6CD8751D46E");

            entity.HasIndex(e => e.ProductCode, "UQ__InvProdu__2F4E024F99C2DAEA").IsUnique();

            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.CostPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Gstpercent)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("GSTPercent");
            entity.Property(e => e.Hsncode)
                .HasMaxLength(20)
                .HasColumnName("HSNCode");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductImagePath).HasMaxLength(500);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ReorderLevel).HasDefaultValue(5);
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.InvProducts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvProduc__Categ__20E1DCB5");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvProducts)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvProduc__UnitI__21D600EE");
        });

        modelBuilder.Entity<InvPurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Poid).HasName("PK__InvPurch__5F02A2D444255B36");

            entity.HasIndex(e => e.Ponumber, "UQ__InvPurch__69B9A84107BA0273").IsUnique();

            entity.Property(e => e.Poid).HasColumnName("POId");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .HasColumnName("PONumber");
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Supplier).WithMany(p => p.InvPurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvPurcha__Suppl__24B26D99");
        });

        modelBuilder.Entity<InvPurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.PoitemId).HasName("PK__InvPurch__CA514790FF093A27");

            entity.Property(e => e.PoitemId).HasColumnName("POItemId");
            entity.Property(e => e.Poid).HasColumnName("POId");
            entity.Property(e => e.Remarks).HasMaxLength(200);
            entity.Property(e => e.TotalCost)
                .HasComputedColumnSql("([OrderQty]*[UnitCostPrice])", false)
                .HasColumnType("decimal(23, 2)");
            entity.Property(e => e.UnitCostPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Po).WithMany(p => p.InvPurchaseOrderItems)
                .HasForeignKey(d => d.Poid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvPurchas__POId__23BE4960");

            entity.HasOne(d => d.Product).WithMany(p => p.InvPurchaseOrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvPurcha__Produ__22CA2527");
        });

        modelBuilder.Entity<InvSaleItem>(entity =>
        {
            entity.HasKey(e => e.SaleItemId).HasName("PK__InvSaleI__C6059401D2091267");

            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Gstpercent)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("GSTPercent");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(200);
            entity.Property(e => e.UnitSellingPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.InvSaleItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvSaleIt__Produ__25A691D2");

            entity.HasOne(d => d.Sale).WithMany(p => p.InvSaleItems)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvSaleIt__SaleI__269AB60B");
        });

        modelBuilder.Entity<InvSaleTransaction>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__InvSaleT__1EE3C3FF373B02F5");

            entity.HasIndex(e => e.BillNumber, "UQ__InvSaleT__C4BBE0C6859A21F3").IsUnique();

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.BalanceDue)
                .HasComputedColumnSql("([TotalAmount]-[AmountPaid])", false)
                .HasColumnType("decimal(13, 2)");
            entity.Property(e => e.BillNumber).HasMaxLength(50);
            entity.Property(e => e.BillType)
                .HasMaxLength(10)
                .HasDefaultValue("Sale");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.CustomerType).HasMaxLength(20);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Gstamount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("GSTAmount");
            entity.Property(e => e.IsPaid).HasDefaultValue(true);
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(20)
                .HasDefaultValue("Cash");
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.SaleDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<InvStockAdjustment>(entity =>
        {
            entity.HasKey(e => e.AdjustmentId).HasName("PK__InvStock__E60DB8938EA0C242");

            entity.Property(e => e.AdjustedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AdjustmentType).HasMaxLength(30);
            entity.Property(e => e.QuantityAfter).HasComputedColumnSql("([QuantityBefore]+[AdjustedQty])", false);
            entity.Property(e => e.Reason).HasMaxLength(300);
            entity.Property(e => e.Remarks).HasMaxLength(300);

            entity.HasOne(d => d.Product).WithMany(p => p.InvStockAdjustments)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvStockA__Produ__278EDA44");
        });

        modelBuilder.Entity<InvStockReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__InvStock__CC08C420512C0F5C");

            entity.HasIndex(e => e.Grnnumber, "UQ__InvStock__8BA9D3858F3823D8").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Grnnumber)
                .HasMaxLength(50)
                .HasColumnName("GRNNumber");
            entity.Property(e => e.InvoiceAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.InvoiceNo).HasMaxLength(100);
            entity.Property(e => e.Poid).HasColumnName("POId");
            entity.Property(e => e.ReceiptDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Received");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Po).WithMany(p => p.InvStockReceipts)
                .HasForeignKey(d => d.Poid)
                .HasConstraintName("FK__InvStockRe__POId__2C538F61");

            entity.HasOne(d => d.Supplier).WithMany(p => p.InvStockReceipts)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvStockR__Suppl__2B5F6B28");
        });

        modelBuilder.Entity<InvStockReceiptItem>(entity =>
        {
            entity.HasKey(e => e.ReceiptItemId).HasName("PK__InvStock__AF7BE10D6DFABDD0");

            entity.Property(e => e.BatchNo).HasMaxLength(50);
            entity.Property(e => e.PoitemId).HasColumnName("POItemId");
            entity.Property(e => e.Remarks).HasMaxLength(200);
            entity.Property(e => e.UnitCostPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Poitem).WithMany(p => p.InvStockReceiptItems)
                .HasForeignKey(d => d.PoitemId)
                .HasConstraintName("FK__InvStockR__POIte__2882FE7D");

            entity.HasOne(d => d.Product).WithMany(p => p.InvStockReceiptItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvStockR__Produ__297722B6");

            entity.HasOne(d => d.Receipt).WithMany(p => p.InvStockReceiptItems)
                .HasForeignKey(d => d.ReceiptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvStockR__Recei__2A6B46EF");
        });

        modelBuilder.Entity<InvSupplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__InvSuppl__4BE666B433D13799");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gstno)
                .HasMaxLength(20)
                .HasColumnName("GSTNo");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SupplierName).HasMaxLength(200);
        });

        modelBuilder.Entity<InvUnit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PK__InvUnits__44F5ECB5C36B1027");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UnitName).HasMaxLength(50);
            entity.Property(e => e.UnitShort).HasMaxLength(10);
        });

        modelBuilder.Entity<LibBook>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__LibBooks__3DE0C207A1F33F25");

            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.BookPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Edition).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.Language)
                .HasMaxLength(50)
                .HasDefaultValue("Hindi");
            entity.Property(e => e.Publisher).HasMaxLength(200);
            entity.Property(e => e.ShelfLocation).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.LibBooks)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LibBooks__Catego__2E3BD7D3");
        });

        modelBuilder.Entity<LibBookCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__LibBookC__19093A0B58E9F911");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LibBookCopy>(entity =>
        {
            entity.HasKey(e => e.CopyId).HasName("PK__LibBookC__C26CCCC574A0EF5C");

            entity.HasIndex(e => e.AccessionNo, "UQ__LibBookC__B4B23BD72A71FA06").IsUnique();

            entity.Property(e => e.AccessionNo).HasMaxLength(50);
            entity.Property(e => e.AcquisitionDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CopyCondition)
                .HasMaxLength(20)
                .HasDefaultValue("Good");
            entity.Property(e => e.CopyPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(500)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.QrcodeImagePath)
                .HasMaxLength(500)
                .HasColumnName("QRCodeImagePath");
            entity.Property(e => e.Remarks).HasMaxLength(300);

            entity.HasOne(d => d.Book).WithMany(p => p.LibBookCopies)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LibBookCo__BookI__2D47B39A");
        });

        modelBuilder.Entity<LibFinePayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__LibFineP__9B556A38A04ECD63");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(30)
                .HasDefaultValue("Cash");
            entity.Property(e => e.ReceiptNo).HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.UserType).HasMaxLength(20);

            entity.HasOne(d => d.Issue).WithMany(p => p.LibFinePayments)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LibFinePa__Issue__2F2FFC0C");
        });

        modelBuilder.Entity<LibFinePolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__LibFineP__2E1339A49A699704");

            entity.ToTable("LibFinePolicy");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DamageFineType)
                .HasMaxLength(20)
                .HasDefaultValue("Percentage");
            entity.Property(e => e.DamageFineValue)
                .HasDefaultValue(50.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IssueDaysForStudent).HasDefaultValue(14);
            entity.Property(e => e.IssueDaysForTeacher).HasDefaultValue(30);
            entity.Property(e => e.LostFineType)
                .HasMaxLength(20)
                .HasDefaultValue("BookPrice");
            entity.Property(e => e.LostFineValue)
                .HasDefaultValue(1.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaxBooksForStudent).HasDefaultValue(2);
            entity.Property(e => e.MaxBooksForTeacher).HasDefaultValue(5);
            entity.Property(e => e.MaxOverdueFine).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PerDayFine)
                .HasDefaultValue(1.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PolicyName).HasMaxLength(100);
        });

        modelBuilder.Entity<LibIssueTransaction>(entity =>
        {
            entity.HasKey(e => e.IssueId).HasName("PK__LibIssue__6C8616044984C8C9");

            entity.Property(e => e.ConditionOnReturn).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FineAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FinePaidDate).HasColumnType("datetime");
            entity.Property(e => e.FineType).HasMaxLength(20);
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.TransactionStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Issued");
            entity.Property(e => e.UserType).HasMaxLength(20);

            entity.HasOne(d => d.Copy).WithMany(p => p.LibIssueTransactions)
                .HasForeignKey(d => d.CopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LibIssueT__CopyI__30242045");

            entity.HasOne(d => d.Policy).WithMany(p => p.LibIssueTransactions)
                .HasForeignKey(d => d.PolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LibIssueT__Polic__3118447E");
        });

        modelBuilder.Entity<LibMemberBlockLog>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("PK__LibMembe__144215F1AA16FAA0");

            entity.ToTable("LibMemberBlockLog");

            entity.Property(e => e.BlockReason).HasMaxLength(500);
            entity.Property(e => e.BlockType).HasMaxLength(30);
            entity.Property(e => e.BlockedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsBlocked).HasDefaultValue(true);
            entity.Property(e => e.UnblockReason).HasMaxLength(300);
            entity.Property(e => e.UnblockedAt).HasColumnType("datetime");
            entity.Property(e => e.UserType).HasMaxLength(20);

            entity.HasOne(d => d.Issue).WithMany(p => p.LibMemberBlockLogs)
                .HasForeignKey(d => d.IssueId)
                .HasConstraintName("FK__LibMember__Issue__320C68B7");
        });

        modelBuilder.Entity<LibSetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PK__LibSetti__01E719AC022224EF");

            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.SettingValue).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__RefreshT__658FEE8A918EF4B4");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__33008CF0");
        });

        modelBuilder.Entity<RoleMaster>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__RoleMast__8AFACE3A634E23FE");

            entity.ToTable("RoleMaster");

            entity.HasIndex(e => e.RoleName, "UQ__RoleMast__8A2B61603DF3D5DF").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<SalaryMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalaryMa__3214EC07F70228CD");

            entity.ToTable("SalaryMaster");

            entity.Property(e => e.BasicSalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Deductions)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.GeneratedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LwpDays).HasColumnName("LWP_Days");
            entity.Property(e => e.MonthYear).HasMaxLength(7);
            entity.Property(e => e.NetSalary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OvertimeAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OvertimeHours).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Generated");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryMasters)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__SalaryMas__Emplo__33F4B129");
        });

        modelBuilder.Entity<TblAcademicSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__Tbl_Acad__C9F49290CC5B12CA");

            entity.ToTable("Tbl_AcademicSession");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SessionName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblAdmission>(entity =>
        {
            entity.HasKey(e => e.AdmissionId).HasName("PK__Tbl_Admi__C97EEC4247E9CB0D");

            entity.ToTable("Tbl_Admission");

            entity.Property(e => e.AdmissionStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AdmissionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Remarks)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany(p => p.TblAdmissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Admis__Stude__34E8D562");
        });

        modelBuilder.Entity<TblAnnouncement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Tbl_Anno__9DE445746D98BF31");

            entity.ToTable("Tbl_Announcement");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsGlobal).HasDefaultValue(false);
            entity.Property(e => e.Message).IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Tbl_Assi__32499E77CA0D0152");

            entity.ToTable("Tbl_Assignment");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblAssignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Assig__Class__35DCF99B");

            entity.HasOne(d => d.Section).WithMany(p => p.TblAssignments)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Assig__Secti__36D11DD4");

            entity.HasOne(d => d.Session).WithMany(p => p.TblAssignments)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Assig__Sessi__37C5420D");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblAssignments)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Assig__Subje__38B96646");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TblAssignments)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Assig__Teach__39AD8A7F");
        });

        modelBuilder.Entity<TblClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Tbl_Clas__CB1927C060E55A1C");

            entity.ToTable("Tbl_Class");

            entity.Property(e => e.ClassName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblClassSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Clas__3214EC07BE834DE6");

            entity.ToTable("Tbl_ClassSection");

            entity.HasIndex(e => new { e.ClassId, e.SectionId, e.SessionId }, "UQ_ClassSection").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblClassSections)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Class__Class__3AA1AEB8");

            entity.HasOne(d => d.PromotionLog).WithMany(p => p.TblClassSections)
                .HasForeignKey(d => d.PromotionLogId)
                .HasConstraintName("FK_ClassSection_PromLog");

            entity.HasOne(d => d.Section).WithMany(p => p.TblClassSections)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Class__Secti__3B95D2F1");

            entity.HasOne(d => d.Session).WithMany(p => p.TblClassSections)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Class__Sessi__3C89F72A");
        });

        modelBuilder.Entity<TblClassSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Clas__3214EC07CC8DF13A");

            entity.ToTable("Tbl_ClassSubject");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblClassSubjects)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Class__Class__3D7E1B63");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblClassSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Class__Subje__3E723F9C");
        });

        modelBuilder.Entity<TblClassworkLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblClass__3214EC07E57E075F");

            entity.ToTable("TblClassworkLog");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TopicCovered).HasMaxLength(500);

            entity.HasOne(d => d.Class).WithMany(p => p.TblClassworkLogs)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classwork_Class");

            entity.HasOne(d => d.Employee).WithMany(p => p.TblClassworkLogs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classwork_Employee");

            entity.HasOne(d => d.Section).WithMany(p => p.TblClassworkLogs)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classwork_Section");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblClassworkLogs)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classwork_Subject");
        });

        modelBuilder.Entity<TblContactQuery>(entity =>
        {
            entity.HasKey(e => e.QueryId).HasName("PK__TblConta__5967F7DB3FAF2CD5");

            entity.ToTable("TblContactQuery");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Mobile).HasMaxLength(15);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblCustomTest>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Tbl_Cust__8CC331603D8F3C48");

            entity.ToTable("Tbl_CustomTest");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxMarks).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TestName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblCustomTests)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Custo__Class__3F6663D5");

            entity.HasOne(d => d.Section).WithMany(p => p.TblCustomTests)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Custo__Secti__405A880E");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblCustomTests)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Custo__Subje__414EAC47");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TblCustomTests)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Custo__Teach__4242D080");
        });

        modelBuilder.Entity<TblCustomTestMark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Cust__3214EC0702A460D0");

            entity.ToTable("Tbl_CustomTestMarks");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsAbsent).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MarksObtained).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany(p => p.TblCustomTestMarks)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Custo__Stude__4336F4B9");

            entity.HasOne(d => d.Test).WithMany(p => p.TblCustomTestMarks)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Custo__TestI__442B18F2");
        });

        modelBuilder.Entity<TblEnquiry>(entity =>
        {
            entity.HasKey(e => e.EnquiryId).HasName("PK__Tbl_Enqu__0A019B7D4118917D");

            entity.ToTable("Tbl_Enquiry");

            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.AlternateMobile)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EnquiryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MobileNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ParentName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Source)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StudentName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.InterestedClass).WithMany(p => p.TblEnquiries)
                .HasForeignKey(d => d.InterestedClassId)
                .HasConstraintName("FK__Tbl_Enqui__Inter__451F3D2B");

            entity.HasOne(d => d.Session).WithMany(p => p.TblEnquiries)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Tbl_Enqui__Sessi__46136164");
        });

        modelBuilder.Entity<TblEnquiryDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Tbl_Enqu__1ABEEF0F4E9065D9");

            entity.ToTable("Tbl_EnquiryDocument");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DocumentUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TblEnquiryDocuments)
                .HasForeignKey(d => d.EnquiryId)
                .HasConstraintName("FK__Tbl_Enqui__Enqui__4707859D");
        });

        modelBuilder.Entity<TblEnquiryFollowUp>(entity =>
        {
            entity.HasKey(e => e.FollowUpId).HasName("PK__Tbl_Enqu__D507D6383B376A5C");

            entity.ToTable("Tbl_EnquiryFollowUp");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FollowUpDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NextFollowUpDate).HasColumnType("datetime");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Enquiry).WithMany(p => p.TblEnquiryFollowUps)
                .HasForeignKey(d => d.EnquiryId)
                .HasConstraintName("FK__Tbl_Enqui__Enqui__47FBA9D6");
        });

        modelBuilder.Entity<TblExam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Tbl_Exam__297521C7AF72063B");

            entity.ToTable("Tbl_Exam");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExamName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Session).WithMany(p => p.TblExams)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Exam__Sessio__48EFCE0F");
        });

        modelBuilder.Entity<TblExamMark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Exam__3214EC07BE785AC2");

            entity.ToTable("Tbl_ExamMarks");

            entity.HasIndex(e => new { e.ExamId, e.StudentId, e.SubjectId }, "UQ_ExamMarks").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsAbsent).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MarksObtained).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Exam).WithMany(p => p.TblExamMarks)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamM__ExamI__49E3F248");

            entity.HasOne(d => d.Student).WithMany(p => p.TblExamMarks)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamM__Stude__4AD81681");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblExamMarks)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamM__Subje__4BCC3ABA");
        });

        modelBuilder.Entity<TblExamSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Exam__3214EC07D3D52825");

            entity.ToTable("Tbl_ExamSubject");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExamTime).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxMarks).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PassMarks).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.RoomNo).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblExamSubjects)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamS__Class__4F9CCB9E");

            entity.HasOne(d => d.Exam).WithMany(p => p.TblExamSubjects)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamS__ExamI__5090EFD7");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblExamSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamS__Subje__51851410");
        });

        modelBuilder.Entity<TblExamWeightage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Exam__3214EC07179077C1");

            entity.ToTable("Tbl_ExamWeightage");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.WeightPct).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Exam).WithMany(p => p.TblExamWeightages)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamW__ExamI__52793849");

            entity.HasOne(d => d.Session).WithMany(p => p.TblExamWeightages)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_ExamW__Sessi__536D5C82");
        });

        modelBuilder.Entity<TblFeeCollection>(entity =>
        {
            entity.HasKey(e => e.FeeCollectionId).HasName("PK__Tbl_FeeC__F9715BC956A0E641");

            entity.ToTable("Tbl_FeeCollection");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FineAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Session).WithMany(p => p.TblFeeCollections)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Tbl_FeeCo__Sessi__546180BB");

            entity.HasOne(d => d.Student).WithMany(p => p.TblFeeCollections)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_FeeCo__Stude__5555A4F4");
        });

        modelBuilder.Entity<TblFeeCollectionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_FeeC__3214EC07D284B52E");

            entity.ToTable("Tbl_FeeCollectionDetails");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.FeeCollection).WithMany(p => p.TblFeeCollectionDetails)
                .HasForeignKey(d => d.FeeCollectionId)
                .HasConstraintName("FK__Tbl_FeeCo__FeeCo__5649C92D");

            entity.HasOne(d => d.FeeType).WithMany(p => p.TblFeeCollectionDetails)
                .HasForeignKey(d => d.FeeTypeId)
                .HasConstraintName("FK__Tbl_FeeCo__FeeTy__573DED66");
        });

        modelBuilder.Entity<TblFeeStructure>(entity =>
        {
            entity.HasKey(e => e.FeeStructureId).HasName("PK__Tbl_FeeS__DDDC25047C7E5D22");

            entity.ToTable("Tbl_FeeStructure");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblFeeStructures)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Tbl_FeeSt__Class__5832119F");

            entity.HasOne(d => d.FeeType).WithMany(p => p.TblFeeStructures)
                .HasForeignKey(d => d.FeeTypeId)
                .HasConstraintName("FK__Tbl_FeeSt__FeeTy__592635D8");

            entity.HasOne(d => d.Session).WithMany(p => p.TblFeeStructures)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Tbl_FeeSt__Sessi__5A1A5A11");
        });

        modelBuilder.Entity<TblFeeTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Tbl_FeeT__55433A6B11D7787C");

            entity.ToTable("Tbl_FeeTransaction");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.FeeCollection).WithMany(p => p.TblFeeTransactions)
                .HasForeignKey(d => d.FeeCollectionId)
                .HasConstraintName("FK__Tbl_FeeTr__FeeCo__5B0E7E4A");
        });

        modelBuilder.Entity<TblFeeType>(entity =>
        {
            entity.HasKey(e => e.FeeTypeId).HasName("PK__Tbl_FeeT__D276A5A02519B429");

            entity.ToTable("Tbl_FeeType");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FeeCategory)
                .HasMaxLength(20)
                .HasDefaultValue("Regular");
            entity.Property(e => e.FeeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblGradeMaster>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Tbl_Grad__54F87A578ECBDFCE");

            entity.ToTable("Tbl_GradeMaster");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GradeName)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.GradePoint).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MinPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblHelpdeskCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblHelpd__3214EC07BE693464");

            entity.ToTable("TblHelpdeskCategory");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblHelpdeskReply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblHelpd__3214EC07B8AAB5D9");

            entity.ToTable("TblHelpdeskReply");

            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ReplyByNavigation).WithMany(p => p.TblHelpdeskReplies)
                .HasForeignKey(d => d.ReplyBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HelpdeskReply_ReplyBy");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TblHelpdeskReplies)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_HelpdeskReply_Ticket");
        });

        modelBuilder.Entity<TblHelpdeskTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblHelpd__3214EC07C1083CD8");

            entity.ToTable("TblHelpdeskTicket");

            entity.HasIndex(e => e.TicketNo, "UQ_HelpdeskTicket_No").IsUnique();

            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Normal");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ResolvedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Open");
            entity.Property(e => e.TicketNo).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TblHelpdeskTickets)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK_HelpdeskTicket_AssignedTo");

            entity.HasOne(d => d.Category).WithMany(p => p.TblHelpdeskTickets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HelpdeskTicket_Category");

            entity.HasOne(d => d.RaisedByNavigation).WithMany(p => p.TblHelpdeskTickets)
                .HasForeignKey(d => d.RaisedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HelpdeskTicket_RaisedBy");
        });

        modelBuilder.Entity<TblIdCardTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__TblIdCar__F87ADD273765EAD0");

            entity.ToTable("TblIdCardTemplate");

            entity.Property(e => e.BackgroundBackPath).HasMaxLength(500);
            entity.Property(e => e.BackgroundFrontPath).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Orientation)
                .HasMaxLength(20)
                .HasDefaultValue("Vertical");
            entity.Property(e => e.PrincipalSignaturePath).HasMaxLength(500);
            entity.Property(e => e.SchoolAddress).HasMaxLength(500);
            entity.Property(e => e.SchoolContact).HasMaxLength(100);
            entity.Property(e => e.SchoolLogoPath).HasMaxLength(500);
            entity.Property(e => e.SchoolName).HasMaxLength(200);
            entity.Property(e => e.TemplateName).HasMaxLength(100);
            entity.Property(e => e.ThemeColor).HasMaxLength(20);
        });

        modelBuilder.Entity<TblLessonCoverage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblLesso__3214EC07BAF6D659");

            entity.ToTable("TblLessonCoverage");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.LessonPlan).WithMany(p => p.TblLessonCoverages)
                .HasForeignKey(d => d.LessonPlanId)
                .HasConstraintName("FK_Coverage_LessonPlan");
        });

        modelBuilder.Entity<TblLessonPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblLesso__3214EC07466E7C9B");

            entity.ToTable("TblLessonPlan");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PlanTitle).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Class).WithMany(p => p.TblLessonPlans)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LessonPlan_Class");

            entity.HasOne(d => d.Employee).WithMany(p => p.TblLessonPlans)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LessonPlan_Employee");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblLessonPlans)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LessonPlan_Subject");

            entity.HasOne(d => d.Topic).WithMany(p => p.TblLessonPlans)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_LessonPlan_Topic");
        });

        modelBuilder.Entity<TblMenu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__TblMenu__C99ED2302380CD5B");

            entity.ToTable("TblMenu");

            entity.Property(e => e.ActionName).HasMaxLength(200);
            entity.Property(e => e.ControllerName).HasMaxLength(200);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MenuName).HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(500);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Menu_Parent");
        });

        modelBuilder.Entity<TblMenuPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__TblMenuP__EFA6FB2FF32B3F61");

            entity.ToTable("TblMenuPermission");

            entity.HasIndex(e => new { e.RoleId, e.MenuId }, "UQ_Role_Menu").IsUnique();

            entity.HasOne(d => d.Menu).WithMany(p => p.TblMenuPermissions)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK_Permission_Menu");

            entity.HasOne(d => d.Role).WithMany(p => p.TblMenuPermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Permission_Role");
        });

        modelBuilder.Entity<TblPeriod>(entity =>
        {
            entity.HasKey(e => e.PeriodId).HasName("PK__Tbl_Peri__E521BB1603B6F2CF");

            entity.ToTable("Tbl_Period");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsBrake).HasDefaultValue(false);
            entity.Property(e => e.PeriodName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblPromotionLog>(entity =>
        {
            entity.ToTable("Tbl_PromotionLog");

            entity.Property(e => e.ExecutedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RolledBackAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Completed");

            entity.HasOne(d => d.SourceSession).WithMany(p => p.TblPromotionLogSourceSessions)
                .HasForeignKey(d => d.SourceSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionLog_Source");

            entity.HasOne(d => d.TargetSession).WithMany(p => p.TblPromotionLogTargetSessions)
                .HasForeignKey(d => d.TargetSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionLog_Target");
        });

        modelBuilder.Entity<TblReportCard>(entity =>
        {
            entity.HasKey(e => e.ReportCardId).HasName("PK__Tbl_Repo__CBAAAA5C36D64D68");

            entity.ToTable("Tbl_ReportCard");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GeneratedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.ObtainedMarks).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.Percentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PublishedDate).HasColumnType("datetime");
            entity.Property(e => e.ResultStatus).HasMaxLength(20);
            entity.Property(e => e.TeacherRemark).HasMaxLength(500);
            entity.Property(e => e.TotalMarks).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.VerificationCode).HasMaxLength(50);

            entity.HasOne(d => d.Class).WithMany(p => p.TblReportCards)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Class__5C02A283");

            entity.HasOne(d => d.Grade).WithMany(p => p.TblReportCards)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Grade__5CF6C6BC");

            entity.HasOne(d => d.Section).WithMany(p => p.TblReportCards)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Secti__5DEAEAF5");

            entity.HasOne(d => d.Session).WithMany(p => p.TblReportCards)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Sessi__5EDF0F2E");

            entity.HasOne(d => d.Student).WithMany(p => p.TblReportCards)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Stude__5FD33367");
        });

        modelBuilder.Entity<TblReportCardSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Repo__3214EC072BAF1392");

            entity.ToTable("Tbl_ReportCardSubject");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxMarks).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.ObtainedMarks).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.Percentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Grade).WithMany(p => p.TblReportCardSubjects)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Grade__60C757A0");

            entity.HasOne(d => d.ReportCard).WithMany(p => p.TblReportCardSubjects)
                .HasForeignKey(d => d.ReportCardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Repor__61BB7BD9");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblReportCardSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Repor__Subje__62AFA012");
        });

        modelBuilder.Entity<TblSection>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__Tbl_Sect__80EF087250A75B6C");

            entity.ToTable("Tbl_Section");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SectionName)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblStudent>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Tbl_Stud__32C52B99EC3C2564");

            entity.ToTable("Tbl_Student");

            entity.Property(e => e.AadhaarNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.AdmissionNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmergencyContactName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EmergencyContactNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PreviousSchool)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RollNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudentName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .HasColumnName("username");
        });

        modelBuilder.Entity<TblStudentAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Tbl_Stud__8B69261C5B8A47AC");

            entity.ToTable("Tbl_StudentAttendance");

            entity.HasIndex(e => new { e.StudentId, e.AttendanceDate, e.SessionId }, "UQ_Attendance").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblStudentAttendances)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Stude__Class__63A3C44B");

            entity.HasOne(d => d.Section).WithMany(p => p.TblStudentAttendances)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Stude__Secti__6497E884");

            entity.HasOne(d => d.Session).WithMany(p => p.TblStudentAttendances)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Stude__Sessi__658C0CBD");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentAttendances)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Stude__Stude__668030F6");
        });

        modelBuilder.Entity<TblStudentDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Tbl_Stud__1ABEEF0F34044346");

            entity.ToTable("Tbl_StudentDocument");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DocumentUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UploadedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentDocuments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__6B44E613");
        });

        modelBuilder.Entity<TblStudentDue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Stud__3214EC07274E3B91");

            entity.ToTable("Tbl_StudentDue");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsSettled).HasDefaultValue(false);
            entity.Property(e => e.PaidAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Remarks)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.SettledDate).HasColumnType("datetime");
            entity.Property(e => e.TotalDue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Session).WithMany(p => p.TblStudentDues)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Tbl_Stude__Sessi__6C390A4C");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentDues)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__6D2D2E85");
        });

        modelBuilder.Entity<TblStudentExit>(entity =>
        {
            entity.ToTable("Tbl_StudentExit");

            entity.Property(e => e.ExitReason).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(d => d.PromotionLog).WithMany(p => p.TblStudentExits)
                .HasForeignKey(d => d.PromotionLogId)
                .HasConstraintName("FK_StudentExit_PromLog");

            entity.HasOne(d => d.Session).WithMany(p => p.TblStudentExits)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentExit_Session");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentExits)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentExit_Student");
        });

        modelBuilder.Entity<TblStudentExtraCharge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Stud__3214EC0710F53585");

            entity.ToTable("Tbl_StudentExtraCharge");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPaid).HasDefaultValue(false);
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.FeeType).WithMany(p => p.TblStudentExtraCharges)
                .HasForeignKey(d => d.FeeTypeId)
                .HasConstraintName("FK__Tbl_Stude__FeeTy__6E2152BE");

            entity.HasOne(d => d.Session).WithMany(p => p.TblStudentExtraCharges)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Tbl_Stude__Sessi__6F1576F7");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentExtraCharges)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__70099B30");
        });

        modelBuilder.Entity<TblStudentFeeOverride>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Stud__3214EC0750EDC766");

            entity.ToTable("Tbl_StudentFeeOverride");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.FeeType).WithMany(p => p.TblStudentFeeOverrides)
                .HasForeignKey(d => d.FeeTypeId)
                .HasConstraintName("FK__Tbl_Stude__FeeTy__70FDBF69");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentFeeOverrides)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__71F1E3A2");
        });

        modelBuilder.Entity<TblStudentMedical>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Stud__3214EC0727033658");

            entity.ToTable("Tbl_StudentMedical");

            entity.Property(e => e.Allergies)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DoctorName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EmergencyContact)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MedicalCondition)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentMedicals)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__72E607DB");
        });

        modelBuilder.Entity<TblStudentOptionalFee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Stud__3214EC07F26C6FC2");

            entity.ToTable("Tbl_StudentOptionalFee");

            entity.HasIndex(e => new { e.StudentId, e.SessionId, e.FeeTypeId }, "UQ_StudentOptionalFee").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.FeeType).WithMany(p => p.TblStudentOptionalFees)
                .HasForeignKey(d => d.FeeTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SOF_FeeType");

            entity.HasOne(d => d.Session).WithMany(p => p.TblStudentOptionalFees)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SOF_Session");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentOptionalFees)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SOF_Student");
        });

        modelBuilder.Entity<TblStudentParent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("PK__Tbl_Stud__D339516F46A56E53");

            entity.ToTable("Tbl_StudentParent");

            entity.Property(e => e.AlternateMobile)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.MobileNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Occupation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ParentName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ParentType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentParents)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__76B698BF");
        });

        modelBuilder.Entity<TblStudentSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Stud__3214EC075BCB960F");

            entity.ToTable("Tbl_StudentSession");

            entity.HasIndex(e => new { e.StudentId, e.SessionId }, "UQ_StudentSession").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PromotionAction).HasMaxLength(20);
            entity.Property(e => e.RetentionReason).HasMaxLength(20);
            entity.Property(e => e.RetentionRemarks).HasMaxLength(300);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblStudentSessions)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Tbl_Stude__Class__77AABCF8");

            entity.HasOne(d => d.PromotionLog).WithMany(p => p.TblStudentSessions)
                .HasForeignKey(d => d.PromotionLogId)
                .HasConstraintName("FK_StudentSession_PromLog");

            entity.HasOne(d => d.Section).WithMany(p => p.TblStudentSessions)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK__Tbl_Stude__Secti__789EE131");

            entity.HasOne(d => d.Session).WithMany(p => p.TblStudentSessions)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Tbl_Stude__Sessi__7993056A");

            entity.HasOne(d => d.Student).WithMany(p => p.TblStudentSessions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Tbl_Stude__Stude__7A8729A3");
        });

        modelBuilder.Entity<TblStudyMaterial>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__Tbl_Stud__C50610F704F9A555");

            entity.ToTable("Tbl_StudyMaterial");

            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblStudyMaterials)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Study__Class__7F4BDEC0");

            entity.HasOne(d => d.Section).WithMany(p => p.TblStudyMaterials)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Study__Secti__004002F9");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblStudyMaterials)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Study__Subje__01342732");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TblStudyMaterials)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Study__Teach__02284B6B");
        });

        modelBuilder.Entity<TblSubject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Tbl_Subj__AC1BA3A8EA8918BD");

            entity.ToTable("Tbl_Subject");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SubjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblSyllabusTopic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblSylla__3214EC077907EB43");

            entity.ToTable("TblSyllabusTopic");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpectedPeriods).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TopicName).HasMaxLength(255);

            entity.HasOne(d => d.Unit).WithMany(p => p.TblSyllabusTopics)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("FK_SyllabusTopic_Unit");
        });

        modelBuilder.Entity<TblSyllabusUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblSylla__3214EC07C50CA4B1");

            entity.ToTable("TblSyllabusUnit");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UnitName).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.TblSyllabusUnits)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyllabusUnit_Class");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblSyllabusUnits)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyllabusUnit_Subject");
        });

        modelBuilder.Entity<TblTeacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Tbl_Teac__EDF259648596DBAE");

            entity.ToTable("Tbl_Teacher");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Designation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MobileNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TeacherName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTeacherAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Teac__3214EC0729179E42");

            entity.ToTable("Tbl_TeacherAssignment");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblTeacherAssignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Teach__Class__031C6FA4");

            entity.HasOne(d => d.Section).WithMany(p => p.TblTeacherAssignments)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Teach__Secti__041093DD");

            entity.HasOne(d => d.Session).WithMany(p => p.TblTeacherAssignments)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Teach__Sessi__0504B816");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblTeacherAssignments)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Teach__Subje__05F8DC4F");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TblTeacherAssignments)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Teach__Teach__06ED0088");
        });

        modelBuilder.Entity<TblTimeTable>(entity =>
        {
            entity.HasKey(e => e.TimeTableId).HasName("PK__Tbl_Time__C087BD0ACFFDD7D8");

            entity.ToTable("Tbl_TimeTable");

            entity.HasIndex(e => new { e.ClassId, e.SectionId, e.DayOfWeek, e.PeriodId, e.SessionId }, "UQ_TimeTable_Class_Period").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_TimeT__Class__07E124C1");

            entity.HasOne(d => d.Period).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.PeriodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_TimeT__Perio__08D548FA");

            entity.HasOne(d => d.PromotionLog).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.PromotionLogId)
                .HasConstraintName("FK_TimeTable_PromLog");

            entity.HasOne(d => d.Section).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_TimeT__Secti__09C96D33");

            entity.HasOne(d => d.Session).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_TimeT__Sessi__0ABD916C");

            entity.HasOne(d => d.Subject).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_TimeT__Subje__0BB1B5A5");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TblTimeTables)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_TimeT__Teach__0CA5D9DE");
        });

        modelBuilder.Entity<TblTrnConductor>(entity =>
        {
            entity.HasKey(e => e.ConductorId).HasName("PK_TrnConductor");

            entity.ToTable("Tbl_TrnConductor");

            entity.Property(e => e.ConductorName).HasMaxLength(100);
            entity.Property(e => e.ContactNumber).HasMaxLength(15);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTrnDriver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PK_TrnDriver");

            entity.ToTable("Tbl_TrnDriver");

            entity.HasIndex(e => e.LicenseNumber, "UQ_TrnDriver_License").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ContactNumber).HasMaxLength(15);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DriverName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LicenseNumber).HasMaxLength(30);
            entity.Property(e => e.PhotoUrl).HasMaxLength(300);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTrnFuelLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnFuelLog");

            entity.ToTable("Tbl_TrnFuelLog");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FuelCostPerLitre).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.FuelQuantityLitres).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.FuelStation).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TotalFuelCost)
                .HasComputedColumnSql("([FuelQuantityLitres]*[FuelCostPerLitre])", false)
                .HasColumnType("decimal(17, 4)");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TblTrnFuelLogs)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnFuelLog_Vehicle");
        });

        modelBuilder.Entity<TblTrnGpsUpdate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnGpsUpdate");

            entity.ToTable("Tbl_TrnGpsUpdate");

            entity.HasIndex(e => new { e.TripId, e.ReceivedAt }, "IX_TrnGpsUpdate_TripId_ReceivedAt").IsDescending(false, true);

            entity.Property(e => e.DeviceTimestamp).HasColumnType("datetime");
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Trip).WithMany(p => p.TblTrnGpsUpdates)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnGps_Trip");
        });

        modelBuilder.Entity<TblTrnMaintenanceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnMaintenance");

            entity.ToTable("Tbl_TrnMaintenanceLog");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ServiceCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServiceProvider).HasMaxLength(200);
            entity.Property(e => e.ServiceType).HasMaxLength(100);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TblTrnMaintenanceLogs)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnMaintenance_Vehicle");
        });

        modelBuilder.Entity<TblTrnNotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnNotifLog");

            entity.ToTable("Tbl_TrnNotificationLog");

            entity.HasIndex(e => new { e.TripId, e.StudentId }, "UQ_TrnNotifLog_TripStudent").IsUnique();

            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Sent");
            entity.Property(e => e.NotificationChannel)
                .HasMaxLength(20)
                .HasDefaultValue("InApp");
            entity.Property(e => e.NotificationSentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Stop).WithMany(p => p.TblTrnNotificationLogs)
                .HasForeignKey(d => d.StopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnNotifLog_Stop");

            entity.HasOne(d => d.Student).WithMany(p => p.TblTrnNotificationLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnNotifLog_Student");

            entity.HasOne(d => d.Trip).WithMany(p => p.TblTrnNotificationLogs)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnNotifLog_Trip");
        });

        modelBuilder.Entity<TblTrnRoute>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK_TrnRoute");

            entity.ToTable("Tbl_TrnRoute");

            entity.HasIndex(e => new { e.RouteName, e.SessionId }, "UQ_TrnRoute_NameSession").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxStudentCapacity).HasDefaultValue((short)40);
            entity.Property(e => e.RouteName).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Session).WithMany(p => p.TblTrnRoutes)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnRoute_Session");
        });

        modelBuilder.Entity<TblTrnRouteStop>(entity =>
        {
            entity.HasKey(e => e.StopId).HasName("PK_TrnRouteStop");

            entity.ToTable("Tbl_TrnRouteStop");

            entity.HasIndex(e => new { e.RouteId, e.StopOrder }, "UQ_TrnRouteStop_Order").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FareAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Landmark).HasMaxLength(200);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.ScheduledArrivalTime).HasPrecision(0);
            entity.Property(e => e.ScheduledDepartureTime).HasPrecision(0);
            entity.Property(e => e.StopName).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Route).WithMany(p => p.TblTrnRouteStops)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnRouteStop_Route");
        });

        modelBuilder.Entity<TblTrnSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnSettings");

            entity.ToTable("Tbl_TrnSettings");

            entity.HasIndex(e => e.SettingKey, "UQ_TrnSettings_Key").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.SettingKey).HasMaxLength(50);
            entity.Property(e => e.SettingValue).HasMaxLength(200);
        });

        modelBuilder.Entity<TblTrnStudentAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnStudentAssignment");

            entity.ToTable("Tbl_TrnStudentAssignment");

            entity.HasIndex(e => new { e.StudentId, e.SessionId }, "UQ_TrnStudentAssign_StudentSession").IsUnique();

            entity.Property(e => e.AssignmentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Route).WithMany(p => p.TblTrnStudentAssignments)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnStudentAssign_Route");

            entity.HasOne(d => d.Session).WithMany(p => p.TblTrnStudentAssignments)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnStudentAssign_Session");

            entity.HasOne(d => d.Stop).WithMany(p => p.TblTrnStudentAssignments)
                .HasForeignKey(d => d.StopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnStudentAssign_Stop");

            entity.HasOne(d => d.Student).WithMany(p => p.TblTrnStudentAssignments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnStudentAssign_Student");
        });

        modelBuilder.Entity<TblTrnTrip>(entity =>
        {
            entity.HasKey(e => e.TripId).HasName("PK_TrnTrip");

            entity.ToTable("Tbl_TrnTrip");

            entity.HasIndex(e => new { e.RouteId, e.TripDate, e.TripType }, "UQ_TrnTrip_RouteDateType").IsUnique();

            entity.Property(e => e.ActualEndTime).HasColumnType("datetime");
            entity.Property(e => e.ActualStartTime).HasColumnType("datetime");
            entity.Property(e => e.AdherenceStatus).HasMaxLength(20);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.SecureToken)
                .HasMaxLength(64)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.TripStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Scheduled");
            entity.Property(e => e.TripType).HasMaxLength(10);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Assignment).WithMany(p => p.TblTrnTrips)
                .HasForeignKey(d => d.AssignmentId)
                .HasConstraintName("FK_TrnTrip_Assignment");

            entity.HasOne(d => d.Route).WithMany(p => p.TblTrnTrips)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnTrip_Route");
        });

        modelBuilder.Entity<TblTrnTripBoardingLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnTripBoardingLog");

            entity.ToTable("Tbl_TrnTripBoardingLog");

            entity.HasIndex(e => new { e.TripId, e.StudentId }, "UQ_TrnBoarding_TripStudent").IsUnique();

            entity.Property(e => e.BoardingStatus)
                .HasMaxLength(10)
                .HasDefaultValue("Unknown");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Stop).WithMany(p => p.TblTrnTripBoardingLogs)
                .HasForeignKey(d => d.StopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnBoarding_Stop");

            entity.HasOne(d => d.Student).WithMany(p => p.TblTrnTripBoardingLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnBoarding_Student");

            entity.HasOne(d => d.Trip).WithMany(p => p.TblTrnTripBoardingLogs)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnBoarding_Trip");
        });

        modelBuilder.Entity<TblTrnVehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK_TrnVehicle");

            entity.ToTable("Tbl_TrnVehicle");

            entity.HasIndex(e => e.RegistrationNumber, "UQ_TrnVehicle_Reg").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Make).HasMaxLength(50);
            entity.Property(e => e.MaxCapacity).HasDefaultValue((short)30);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.PhotoUrl).HasMaxLength(300);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(30);
            entity.Property(e => e.Remarks).HasMaxLength(300);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.VehicleType).HasMaxLength(20);
        });

        modelBuilder.Entity<TblTrnVehicleAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK_TrnVehicleAssignment");

            entity.ToTable("Tbl_TrnVehicleAssignment");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Conductor).WithMany(p => p.TblTrnVehicleAssignments)
                .HasForeignKey(d => d.ConductorId)
                .HasConstraintName("FK_TrnVehicleAssign_Cond");

            entity.HasOne(d => d.Driver).WithMany(p => p.TblTrnVehicleAssignments)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnVehicleAssign_Driver");

            entity.HasOne(d => d.Route).WithMany(p => p.TblTrnVehicleAssignments)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnVehicleAssign_Route");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TblTrnVehicleAssignments)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnVehicleAssign_Vehicle");
        });

        modelBuilder.Entity<TblTrnWeeklySchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrnWeeklySchedule");

            entity.ToTable("Tbl_TrnWeeklySchedule");

            entity.HasIndex(e => new { e.RouteId, e.DayOfWeek, e.TripType }, "UQ_TrnWeeklySchedule").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TripType).HasMaxLength(10);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Route).WithMany(p => p.TblTrnWeeklySchedules)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrnWeeklySchedule_Route");
        });

        modelBuilder.Entity<UserMaster>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserMast__1788CCAC3D8B177B");

            entity.ToTable("UserMaster");

            entity.HasIndex(e => e.Username, "UQ__UserMast__536C85E4D4F3C083").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__UserMast__A9D10534F71A8CB8").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmpId).HasColumnName("empId");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Emp).WithMany(p => p.UserMasters)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_UserMaster_Employee");

            entity.HasOne(d => d.Student).WithMany(p => p.UserMasters)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_UserMaster_Student");

            entity.HasOne(d => d.Teacher).WithMany(p => p.UserMasters)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_UserMaster_Teacher");
        });

        modelBuilder.Entity<UserOtp>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__UserOtp__3143C4A355518FDC");

            entity.ToTable("UserOtp");

            entity.Property(e => e.CreatedDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiryDateTime).HasColumnType("datetime");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OtpCode)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.VerificationToken)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserOtps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserOtp_UserMaster");
        });

        modelBuilder.Entity<UserRoleAssign>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__UserRole__32499E578882DBCA");

            entity.ToTable("UserRoleAssign");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRole").IsUnique();

            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoleAssigns)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRoleA__RoleI__19FFD4FC");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoleAssigns)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRoleA__UserI__1AF3F935");
        });

        modelBuilder.Entity<VwLibBookStock>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_LibBookStock");

            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ShelfLocation).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(300);
        });

        modelBuilder.Entity<VwLibCurrentIssue>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_LibCurrentIssues");

            entity.Property(e => e.AccessionNo).HasMaxLength(50);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.FineAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.TransactionStatus).HasMaxLength(20);
            entity.Property(e => e.UserType).HasMaxLength(20);
        });

        modelBuilder.Entity<VwLibMemberBlockStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_LibMemberBlockStatus");

            entity.Property(e => e.LastBlockedAt).HasColumnType("datetime");
            entity.Property(e => e.LatestBlockType).HasMaxLength(30);
            entity.Property(e => e.UserType).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
