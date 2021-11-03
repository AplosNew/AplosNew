using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PhysicalStockAdjustmentHistoryConfiguration : EntityTypeConfiguration<PhysicalStockAdjustmentHistory>
    {
        public PhysicalStockAdjustmentHistoryConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Qty).HasPrecision(18, 10);
            Property(t => t.Rate).HasPrecision(18, 10);
            ToTable(nameof(PhysicalStockAdjustmentHistory), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}