using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseDocAcceptanceTaxConfiguration : EntityTypeConfiguration<PurchaseDocAcceptanceTax>
    {
        public PurchaseDocAcceptanceTaxConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Percentage).HasPrecision(18, 10);
            Property(t => t.TaxAmount).HasPrecision(18, 10);
            ToTable(nameof(PurchaseDocAcceptanceTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.ServiceMasterId);
            Ignore(r => r.AcceptanceServiceId);
        }
    }
}