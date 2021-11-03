using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryScrapDetailConfiguration : EntityTypeConfiguration<InventoryScrapDetail> 
    {
        public InventoryScrapDetailConfiguration() 
        {
            HasKey(t => t.Id);
            Property(t => t.AvgRate).HasPrecision(18, 10);
            Property(t => t.AvgAmount).HasPrecision(18, 10);
            Property(t => t.PolicyRate).HasPrecision(18, 10);
            Property(t => t.PolicyAmount).HasPrecision(18, 10);
            ToTable(nameof(InventoryScrapDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}