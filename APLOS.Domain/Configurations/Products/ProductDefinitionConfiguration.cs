using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ProductDefinitionConfiguration : EntityTypeConfiguration<ProductDefinition>
    {
        public ProductDefinitionConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductDefinition), DbSchema.Transaction);
        }
    }
}