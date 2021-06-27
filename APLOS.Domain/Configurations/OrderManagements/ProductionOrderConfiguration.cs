using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderConfiguration : EntityTypeConfiguration<ProductionOrder>
    {
        public ProductionOrderConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductionOrder), DbSchema.Transaction);
        }
    }
}