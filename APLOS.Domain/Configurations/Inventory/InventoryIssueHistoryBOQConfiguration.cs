using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryIssueHistoryBOQConfiguration : EntityTypeConfiguration<InventoryIssueHistoryBOQ>
    {
        public InventoryIssueHistoryBOQConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Qty).HasPrecision(18, 10);
            Property(t => t.Rate).HasPrecision(18, 4);
            ToTable(nameof(InventoryIssueHistoryBOQ), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.RequisitionQty);
        }
    }
}