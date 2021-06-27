using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderWorkCenterConfiguration : EntityTypeConfiguration<ProductionOrderWorkCenter>
    {
        public ProductionOrderWorkCenterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            // Table & Column Configuration
            ToTable(nameof(ProductionOrderWorkCenter), DbSchema.Transaction);
        }
    }
}