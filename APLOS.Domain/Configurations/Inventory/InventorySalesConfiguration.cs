using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventorySalesConfiguration : EntityTypeConfiguration<InventorySales> 
    {
        public InventorySalesConfiguration() 
        {
            HasKey(t => t.Id);
            ToTable(nameof(InventorySales), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}