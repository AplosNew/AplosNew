using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderProcessSetConfiguration : EntityTypeConfiguration<ProductionOrderProcessSet>
    {
        public ProductionOrderProcessSetConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductionOrderProcessSet), DbSchema.Transaction);
        }
    }
}