using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventorySalesReturnConfiguration : EntityTypeConfiguration<InventorySalesReturn> 
    {
        public InventorySalesReturnConfiguration() 
        {
            HasKey(t => t.Id);
            ToTable(nameof(InventorySalesReturn), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}