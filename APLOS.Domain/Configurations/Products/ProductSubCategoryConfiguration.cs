using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductSubCategoryConfiguration : EntityTypeConfiguration<ProductSubCategory>
    {
        public ProductSubCategoryConfiguration()
        {
            ToTable(nameof(ProductSubCategory), DbSchema.HKP);
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
        }
    }
}