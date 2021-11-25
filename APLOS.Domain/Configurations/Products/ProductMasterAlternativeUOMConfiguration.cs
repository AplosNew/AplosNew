using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Materials
{
    public class ProductMasterAlternativeUOMConfiguration : EntityTypeConfiguration<ProductMasterAlternativeUOM>
    {
        public ProductMasterAlternativeUOMConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Property(t => t.BaseUOMFactor).HasPrecision(18, 8);
            // Table & Column Configuration
            ToTable(nameof(ProductMasterAlternativeUOM), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}