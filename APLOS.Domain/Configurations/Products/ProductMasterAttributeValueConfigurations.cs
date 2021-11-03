using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductMasterAttributeValueConfiguration : EntityTypeConfiguration<ProductMasterAttributeValue>
    {
        public ProductMasterAttributeValueConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductMasterAttributeValue), DbSchema.Masters);
            HasKey(t => t.Id);
        }
    }
}