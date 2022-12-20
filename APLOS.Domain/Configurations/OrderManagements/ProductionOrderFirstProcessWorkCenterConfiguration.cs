using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductionOrderFirstProcessWorkCenterConfiguration : EntityTypeConfiguration<ProductionOrderFirstProcessWorkCenter>
    {
        public ProductionOrderFirstProcessWorkCenterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            // Table & Column Configuration
            ToTable(nameof(ProductionOrderFirstProcessWorkCenter), DbSchema.Dbo);
        }
    }
}