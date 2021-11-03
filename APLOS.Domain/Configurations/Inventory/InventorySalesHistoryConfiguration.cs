using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventorySalesHistoryConfiguration : EntityTypeConfiguration<InventorySalesHistory>
    {
        public InventorySalesHistoryConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Qty).HasPrecision(18, 10);
            Property(t => t.BaseRate).HasPrecision(18, 10);
            ToTable(nameof(InventorySalesHistory), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}