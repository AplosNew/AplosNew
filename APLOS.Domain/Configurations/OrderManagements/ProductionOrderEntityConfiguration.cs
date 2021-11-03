using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderEntityConfiguration : EntityTypeConfiguration<ProductionOrderEntity>
    {
        public ProductionOrderEntityConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductionOrderEntity), DbSchema.Transaction);
        }
    }
}