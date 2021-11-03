using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseReturnServiceConfiguration : EntityTypeConfiguration<PurchaseReturnService>
    {
        public PurchaseReturnServiceConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Amount).HasPrecision(18, 2);
            Property(t => t.TotalTaxAmount).HasPrecision(18, 2);
            ToTable(nameof(PurchaseReturnService), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}