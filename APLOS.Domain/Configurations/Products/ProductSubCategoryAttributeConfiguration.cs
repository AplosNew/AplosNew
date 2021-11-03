using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductSubCategoryAttributeConfiguration : EntityTypeConfiguration<ProductSubCategoryAttribute>
    {
        public ProductSubCategoryAttributeConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProductSubCategoryAttribute), DbSchema.Masters);
        }
    }
}