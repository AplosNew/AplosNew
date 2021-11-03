using Library.Model.Enums;
using Library.Model.Products;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductGroupConfiguration : EntityTypeConfiguration<ProductGroup>
    {
        public ProductGroupConfiguration()
        {
            // Table & Column Configuration
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductGroup), DbSchema.HKP);

            // Primary Key
            HasKey(t => t.Id);

            #region Properties

            Property(t => t.Id)
                .HasColumnName("Id")
                .HasMaxLength(10)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(t => t.Sequence)
                .HasColumnName("Sequence")
                .HasPrecision(18, 2);

            Property(t => t.Code)
                .HasColumnName("Code")
                .HasMaxLength(10);

            Property(t => t.ShortName)
                 .HasColumnName("ShortName")
                 .HasMaxLength(15);

            Property(t => t.StandardName)
                 .HasColumnName("StandardName")
                 .HasMaxLength(50);

            Property(t => t.UserName)
                 .HasColumnName("UserName")
                 .HasMaxLength(50);

            Property(t => t.Description)
                 .HasColumnName("Description")
                 .HasMaxLength(250);

            Property(t => t.Remarks)
                 .HasColumnName("Remarks")
                 .HasMaxLength(250);

            Property(t => t.IsActive)
                .HasColumnName("Active");

            Property(t => t.IsArchive)
                .HasColumnName("Archive");

            #endregion

            #region Audit Configuration

            Property(t => t.AddedBy)
                       .HasColumnName("ADDEDBY")
                       .IsRequired()
                       .HasMaxLength(30);

            Property(t => t.AddedDate)
                        .HasColumnName("ADDEDDATE")
                        .HasColumnType("DateTime")
                        .IsRequired();

            Property(t => t.AddedFromIP)
                        .HasColumnName("ADDEDFROMIP")
                        .IsRequired()
                        .HasMaxLength(15);

            Property(t => t.UpdatedBy)
                        .HasColumnName("UPDATEDBY")
                        .HasMaxLength(30);

            Property(t => t.UpdatedDate)
                        .HasColumnName("UPDATEDDATE")
                        .HasColumnType("DateTime");

            Property(t => t.UpdatedFromIP)
                        .HasColumnName("UPDATEDFROMIP")
                        .HasMaxLength(15);

            #endregion
        }
    }
}