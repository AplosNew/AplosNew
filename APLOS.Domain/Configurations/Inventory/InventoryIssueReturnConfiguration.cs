using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryIssueReturnConfiguration : EntityTypeConfiguration<InventoryIssueReturn>
    {
        public InventoryIssueReturnConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(InventoryIssueReturn), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}