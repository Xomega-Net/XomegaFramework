//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Services.Entities
{
    public class WorkOrderRoutingConfig : IEntityTypeConfiguration<WorkOrderRouting>
    {
        public void Configure(EntityTypeBuilder<WorkOrderRouting> c)
        {
            c.ToTable("WorkOrderRouting", "Production")
             .HasKey(e => new { e.WorkOrderId, e.ProductId, e.OperationSequence });

            // configure relationships

            c.HasOne(e => e.WorkOrderObject)
             .WithOne()
             .HasForeignKey<WorkOrderRouting>(e => e.WorkOrderId);

            c.HasOne(e => e.LocationObject)
             .WithMany()
             .HasForeignKey(e => e.LocationId);

            // configure properties
          
            c.Property(e => e.WorkOrderId)
             .HasColumnName("WorkOrderID")
             .HasColumnType("int")
             .IsRequired();

            c.Property(e => e.ProductId)
             .HasColumnName("ProductID")
             .HasColumnType("int")
             .IsRequired();

            c.Property(e => e.OperationSequence)
             .HasColumnName("OperationSequence")
             .HasColumnType("smallint")
             .IsRequired();

            c.Property(e => e.LocationId)
             .HasColumnName("LocationID")
             .HasColumnType("smallint")
             .IsRequired();

            c.Property(e => e.ScheduledStartDate)
             .HasColumnName("ScheduledStartDate")
             .HasColumnType("datetime")
             .IsRequired();

            c.Property(e => e.ScheduledEndDate)
             .HasColumnName("ScheduledEndDate")
             .HasColumnType("datetime")
             .IsRequired();

            c.Property(e => e.ActualStartDate)
             .HasColumnName("ActualStartDate")
             .HasColumnType("datetime");

            c.Property(e => e.ActualEndDate)
             .HasColumnName("ActualEndDate")
             .HasColumnType("datetime");

            c.Property(e => e.ActualResourceHrs)
             .HasColumnName("ActualResourceHrs")
             .HasColumnType("decimal(9,4)");

            c.Property(e => e.PlannedCost)
             .HasColumnName("PlannedCost")
             .HasColumnType("money")
             .IsRequired();

            c.Property(e => e.ActualCost)
             .HasColumnName("ActualCost")
             .HasColumnType("money");

            c.Property(e => e.ModifiedDate)
             .HasColumnName("ModifiedDate")
             .HasColumnType("datetime")
             .IsRequired();

        }
    }
}