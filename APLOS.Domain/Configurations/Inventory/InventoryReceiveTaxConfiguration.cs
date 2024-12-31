using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class InventoryReceiveTaxConfiguration : EntityTypeConfiguration<InventoryReceiveTax>
    {
        public InventoryReceiveTaxConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Percentage).HasPrecision(18, 10);
            Property(t => t.TaxAmount).HasPrecision(18, 2);
            ToTable(nameof(InventoryReceiveTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.PODetailsID);
            Ignore(r => r.RowIdentityNo);
        }
    }
}