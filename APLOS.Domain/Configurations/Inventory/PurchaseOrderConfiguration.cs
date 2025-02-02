using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseOrderConfiguration : EntityTypeConfiguration<PurchaseOrder>
    {
        public PurchaseOrderConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            ToTable(nameof(PurchaseOrder), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.Amount);
        }
    }
}