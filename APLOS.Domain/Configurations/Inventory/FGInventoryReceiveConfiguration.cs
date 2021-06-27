using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class FGInventoryReceiveConfiguration : EntityTypeConfiguration<FGInventoryReceive>
    {
        public FGInventoryReceiveConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(FGInventoryReceive), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}