using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventorySalesReturnTaxConfiguration : EntityTypeConfiguration<InventorySalesReturnTax>  
    {
        public InventorySalesReturnTaxConfiguration()  
        {
            HasKey(t => t.Id);
            ToTable(nameof(InventorySalesReturnTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}