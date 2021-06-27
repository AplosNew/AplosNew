using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseReturnTaxConfiguration : EntityTypeConfiguration<PurchaseReturnTax>
    {
        public PurchaseReturnTaxConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Percentage).HasPrecision(18, 4);
            Property(t => t.TaxAmount).HasPrecision(18, 2);
            ToTable(nameof(PurchaseReturnTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.PODetailId);
            Ignore(r => r.ServiceMasterId);

        }
    }
}