using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PhysicalStockAdjustmentMasterConfiguration : EntityTypeConfiguration<PhysicalStockAdjustmentMaster>
    {
        public PhysicalStockAdjustmentMasterConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(PhysicalStockAdjustmentMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}