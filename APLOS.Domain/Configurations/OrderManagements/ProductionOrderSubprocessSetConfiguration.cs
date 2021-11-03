using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderSubprocessSetConfiguration : EntityTypeConfiguration<ProductionOrderSubprocessSet>
    {
        public ProductionOrderSubprocessSetConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductionOrderSubprocessSet), DbSchema.Transaction);
        }
    }
}