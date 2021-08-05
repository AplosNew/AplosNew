using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventorySalesReturnDetailConfiguration : EntityTypeConfiguration<InventorySalesReturnDetail>
    {
        public InventorySalesReturnDetailConfiguration() 
        {
            HasKey(t => t.Id);
            Property(t => t.AvgRate).HasPrecision(18, 10);
            Property(t => t.AvgAmount).HasPrecision(18, 10);
            
            ToTable(nameof(InventorySalesReturnDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}