using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductMasterConfiguration : EntityTypeConfiguration<ProductMaster>
    {
        public ProductMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductMaster), DbSchema.Masters);
            HasKey(t => t.Id);
        }
    }
}