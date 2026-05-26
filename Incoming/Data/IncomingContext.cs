using System;
using System.Collections.Generic;
using Incoming.Models;
using Microsoft.EntityFrameworkCore;

namespace Incoming.Data;

public partial class IncomingContext : DbContext
{
    public IncomingContext()
    {
    }

    public IncomingContext(DbContextOptions<IncomingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcceptedPiece> AcceptedPieces { get; set; }

    public virtual DbSet<Container> Containers { get; set; }

    public virtual DbSet<Defect> Defects { get; set; }

    public virtual DbSet<Finishedgood> Finishedgoods { get; set; }

    public virtual DbSet<InspectionProgram> InspectionPrograms { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<Rejection> Rejections { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ViewParameter> ViewParameters { get; set; }

    public virtual DbSet<ViewRejection> ViewRejections { get; set; }

    public virtual DbSet<ViewRejectionmaterial> ViewRejectionmaterials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcceptedPiece>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ACCEPTED__3214EC27XXXXXXXX");

            entity.ToTable("ACCEPTED_PIECES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Base)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BASE");
            entity.Property(e => e.Connector)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CONNECTOR");
            entity.Property(e => e.Gap)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GAP");
            entity.Property(e => e.Guard)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GUARD");
            entity.Property(e => e.Heater)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HEATER");
            entity.Property(e => e.Inside)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("INSIDE");
            entity.Property(e => e.Molding)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MOLDING");
            entity.Property(e => e.Ntc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NTC");
            entity.Property(e => e.Outside)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("OUTSIDE");
            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM");
            entity.Property(e => e.AcceptedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ACCEPTED_DATE");
        });

        modelBuilder.Entity<Container>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CONTAINE__3214EC277C6B75A5");

            entity.ToTable("CONTAINERS");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime")
                .HasColumnName("DATE_END");
            entity.Property(e => e.DateStart)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("DATE_START");
            entity.Property(e => e.PartialCount).HasColumnName("PARTIAL_COUNT");
            entity.Property(e => e.Program)
                .HasMaxLength(100)
                .HasColumnName("PROGRAM");
            entity.Property(e => e.Quantity).HasColumnName("QUANTITY");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("OPEN")
                .HasColumnName("STATUS");
        });

        modelBuilder.Entity<Defect>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DEFECT__3214EC271CD9BC01");

            entity.ToTable("DEFECT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DefectName)
                .HasMaxLength(100)
                .HasColumnName("DEFECT_NAME");
        });

        modelBuilder.Entity<Finishedgood>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FINISHED__3214EC27F5B32CDF");

            entity.ToTable("FINISHEDGOOD");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Base)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BASE");
            entity.Property(e => e.Guard)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GUARD");
            entity.Property(e => e.Heater)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("HEATER");
            entity.Property(e => e.Inside)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("INSIDE");
            entity.Property(e => e.Ntc)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NTC");
            entity.Property(e => e.Outside)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("OUTSIDE");
            entity.Property(e => e.Partnumber)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PARTNUMBER");
            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM");
            entity.Property(e => e.RejectionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("REJECTION_DATE");
        });

        modelBuilder.Entity<InspectionProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PROGRAMS__3214EC2727F74A9E");

            entity.ToTable("INSPECTION_PROGRAMS");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM_NAME");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Parameter");

            entity.ToTable("PARAMETER");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Maximum)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("MAXIMUM");
            entity.Property(e => e.Minimum)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("MINIMUM");
            entity.Property(e => e.ProgramId).HasColumnName("PROGRAM_ID");
            entity.Property(e => e.Test)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TEST");

            entity.HasOne(d => d.Program).WithMany(p => p.Parameters)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK_PARAMETER_PROGRAM");
        });

        modelBuilder.Entity<Rejection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REJECTIO__3214EC273EB18859");

            entity.ToTable("REJECTION");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Base)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BASE");
            entity.Property(e => e.Connector)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CONNECTOR");
            entity.Property(e => e.Gap)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GAP");
            entity.Property(e => e.Guard)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GUARD");
            entity.Property(e => e.Heater)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HEATER");
            entity.Property(e => e.Inside)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("INSIDE");
            entity.Property(e => e.Molding)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MOLDING");
            entity.Property(e => e.Ntc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NTC");
            entity.Property(e => e.Outside)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("OUTSIDE");
            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM");
            entity.Property(e => e.RejectionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("REJECTION_DATE");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USERS__3214EC27657352BA");

            entity.ToTable("USERS");

            entity.HasIndex(e => e.EmployeeNumber, "UQ__USERS__E96A55247F910086").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Area)
                .HasMaxLength(100)
                .HasColumnName("AREA");
            entity.Property(e => e.EmployeeNumber)
                .HasMaxLength(50)
                .HasColumnName("EMPLOYEE_NUMBER");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("FIRST_NAME");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("LAST_NAME");
            entity.Property(e => e.UserRole)
                .HasMaxLength(50)
                .HasColumnName("USER_ROLE");
        });

        modelBuilder.Entity<ViewParameter>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_PARAMETERS");

            entity.Property(e => e.Maximum)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("MAXIMUM");
            entity.Property(e => e.Minimum)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("MINIMUM");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM_NAME");
            entity.Property(e => e.Test)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TEST");
        });

        modelBuilder.Entity<ViewRejection>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_REJECTIONS");

            entity.Property(e => e.Base)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BASE");
            entity.Property(e => e.Connector)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CONNECTOR");
            entity.Property(e => e.Gap)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GAP");
            entity.Property(e => e.Guard)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GUARD");
            entity.Property(e => e.Heater)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HEATER");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Inside)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("INSIDE");
            entity.Property(e => e.Molding)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MOLDING");
            entity.Property(e => e.Ntc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NTC");
            entity.Property(e => e.Outside)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("OUTSIDE");
            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM");
            entity.Property(e => e.RejectionDate)
                .HasMaxLength(4000)
                .HasColumnName("REJECTION_DATE");
            entity.Property(e => e.RejectionTime)
                .HasMaxLength(4000)
                .HasColumnName("REJECTION_TIME");
        });

        modelBuilder.Entity<ViewRejectionmaterial>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_REJECTIONMATERIAL");

            entity.Property(e => e.Base)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BASE");
            entity.Property(e => e.Category)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("CATEGORY");
            entity.Property(e => e.Connector)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CONNECTOR");
            entity.Property(e => e.Gap)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GAP");
            entity.Property(e => e.Guard)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GUARD");
            entity.Property(e => e.Heater)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("HEATER");
            entity.Property(e => e.Inside)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("INSIDE");
            entity.Property(e => e.Molding)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MOLDING");
            entity.Property(e => e.Ntc)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NTC");
            entity.Property(e => e.Outside)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("OUTSIDE");
            entity.Property(e => e.Partnumber)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PARTNUMBER");
            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PROGRAM");
            entity.Property(e => e.RejectionDate)
                .HasColumnType("datetime")
                .HasColumnName("REJECTION_DATE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
