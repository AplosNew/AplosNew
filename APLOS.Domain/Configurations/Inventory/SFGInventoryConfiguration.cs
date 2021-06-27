using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class SFGInventoryConfiguration : EntityTypeConfiguration<SFGInventory>
    {
        public SFGInventoryConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(SFGInventory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}