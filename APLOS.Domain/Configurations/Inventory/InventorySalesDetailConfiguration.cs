using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventorySalesDetailConfiguration : EntityTypeConfiguration<InventorySalesDetail>
    {
        public InventorySalesDetailConfiguration() 
        {
            HasKey(t => t.Id);
            Property(t => t.AvgRate).HasPrecision(18, 10);
            Property(t => t.AvgAmount).HasPrecision(20, 10);
            Property(t => t.PolicyRate).HasPrecision(18, 10);
            Property(t => t.PolicyAmount).HasPrecision(20, 10);
            Property(t => t.SalesRate).HasPrecision(18, 10);
            ToTable(nameof(InventorySalesDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}