//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Services.Entities
{
    public class ErrorLogConfig : IEntityTypeConfiguration<ErrorLog>
    {
        public void Configure(EntityTypeBuilder<ErrorLog> c)
        {
            c.ToTable("ErrorLog")
             .HasKey(e => e.ErrorLogId);

            // configure properties
          
            c.Property(e => e.ErrorLogId)
             .HasColumnName("ErrorLogID")
             .HasColumnType("int")
             .IsRequired()
             .ValueGeneratedOnAdd();

            c.Property(e => e.ErrorTime)
             .HasColumnName("ErrorTime")
             .HasColumnType("datetime")
             .IsRequired();

            c.Property(e => e.UserName)
             .HasColumnName("UserName")
             .HasColumnType("nvarchar")
             .HasMaxLength(128)
             .IsUnicode()
             .IsRequired();

            c.Property(e => e.ErrorNumber)
             .HasColumnName("ErrorNumber")
             .HasColumnType("int")
             .IsRequired();

            c.Property(e => e.ErrorSeverity)
             .HasColumnName("ErrorSeverity")
             .HasColumnType("int");

            c.Property(e => e.ErrorState)
             .HasColumnName("ErrorState")
             .HasColumnType("int");

            c.Property(e => e.ErrorProcedure)
             .HasColumnName("ErrorProcedure")
             .HasColumnType("nvarchar")
             .HasMaxLength(126)
             .IsUnicode();

            c.Property(e => e.ErrorLine)
             .HasColumnName("ErrorLine")
             .HasColumnType("int");

            c.Property(e => e.ErrorMessage)
             .HasColumnName("ErrorMessage")
             .HasColumnType("nvarchar")
             .HasMaxLength(4000)
             .IsUnicode()
             .IsRequired();

        }
    }
}