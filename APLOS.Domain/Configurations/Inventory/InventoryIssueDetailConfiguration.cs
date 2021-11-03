using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryIssueDetailConfiguration : EntityTypeConfiguration<InventoryIssueDetail>
    {
        public InventoryIssueDetailConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.AvgRate).HasPrecision(18, 4);
            Property(t => t.AvgAmount).HasPrecision(18, 2);
            Property(t => t.PolicyRate).HasPrecision(18, 4);
            Property(t => t.PolicyAmount).HasPrecision(18, 2);
            ToTable(nameof(InventoryIssueDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}