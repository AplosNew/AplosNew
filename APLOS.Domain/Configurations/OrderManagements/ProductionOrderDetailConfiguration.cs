using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderDetailConfiguration : EntityTypeConfiguration<ProductionOrderDetail>
    {
        public ProductionOrderDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductionOrderDetail), DbSchema.Transaction);
        }
    }
}