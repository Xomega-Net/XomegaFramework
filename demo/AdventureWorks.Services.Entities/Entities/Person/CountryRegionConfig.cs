//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Services.Entities
{
    public class CountryRegionConfig : IEntityTypeConfiguration<CountryRegion>
    {
        public void Configure(EntityTypeBuilder<CountryRegion> c)
        {
            c.ToTable("CountryRegion", "Person")
             .HasKey(e => e.CountryRegionCode);

            // configure properties
          
            c.Property(e => e.CountryRegionCode)
             .HasColumnName("CountryRegionCode")
             .HasColumnType("nvarchar")
             .HasMaxLength(3)
             .IsUnicode()
             .IsRequired();

            c.Property(e => e.Name)
             .HasColumnName("Name")
             .HasColumnType("nvarchar")
             .HasMaxLength(50)
             .IsUnicode()
             .IsRequired();

            c.Property(e => e.ModifiedDate)
             .HasColumnName("ModifiedDate")
             .HasColumnType("datetime")
             .IsRequired();

        }
    }
}