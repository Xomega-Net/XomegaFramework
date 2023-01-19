//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Services.Entities
{
    public class ShipMethodConfig : IEntityTypeConfiguration<ShipMethod>
    {
        public void Configure(EntityTypeBuilder<ShipMethod> c)
        {
            c.ToTable("ShipMethod", "Purchasing")
             .HasKey(e => e.ShipMethodId);

            // configure properties
          
            c.Property(e => e.ShipMethodId)
             .HasColumnName("ShipMethodID")
             .HasColumnType("int")
             .IsRequired()
             .ValueGeneratedOnAdd();

            c.Property(e => e.Name)
             .HasColumnName("Name")
             .HasColumnType("nvarchar")
             .HasMaxLength(50)
             .IsUnicode()
             .IsRequired();

            c.Property(e => e.ShipBase)
             .HasColumnName("ShipBase")
             .HasColumnType("money")
             .IsRequired();

            c.Property(e => e.ShipRate)
             .HasColumnName("ShipRate")
             .HasColumnType("money")
             .IsRequired();

            c.Property(e => e.Rowguid)
             .HasColumnName("rowguid")
             .HasColumnType("uniqueidentifier")
             .IsRequired();

            c.Property(e => e.ModifiedDate)
             .HasColumnName("ModifiedDate")
             .HasColumnType("datetime")
             .IsRequired();

        }
    }
}