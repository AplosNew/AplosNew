using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryScrapConfiguration : EntityTypeConfiguration<InventoryScrap>  
    {
        public InventoryScrapConfiguration()  
        {
            HasKey(t => t.Id);
            ToTable(nameof(InventoryScrap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}