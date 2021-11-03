using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryReceiveConfiguration : EntityTypeConfiguration<InventoryReceive>
    {
        public InventoryReceiveConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            ToTable(nameof(InventoryReceive), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.FromMaterialStorageId);
            Ignore(r => r.msgForAllocationNeed);
             

        }
    }
}