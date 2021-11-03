using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductMasterEfficencyConfiguration : EntityTypeConfiguration<ProductMasterEfficency>
    {
        public ProductMasterEfficencyConfiguration()
        {
            Ignore(t => t.ModelState);
            Property(t => t.EfficencyPercentage).HasPrecision(18, 4);
            HasKey(t => t.Id);
            ToTable(nameof(ProductMasterEfficency), DbSchema.Transaction);
        }
    }
}