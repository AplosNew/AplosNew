using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryIssueConfiguration : EntityTypeConfiguration<InventoryIssue>
    {
        public InventoryIssueConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(InventoryIssue), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}