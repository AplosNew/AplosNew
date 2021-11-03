using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class EntitySFGInventoryConfiguration : EntityTypeConfiguration<EntitySFGInventory>
    {
        public EntitySFGInventoryConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(EntitySFGInventory), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}