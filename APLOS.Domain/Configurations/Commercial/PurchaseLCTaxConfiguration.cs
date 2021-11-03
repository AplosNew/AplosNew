using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Commercial
{
    public class PurchaseLCTaxConfiguration : EntityTypeConfiguration<PurchaseLCTax>
    {
        public PurchaseLCTaxConfiguration()
        {
            ToTable(nameof(PurchaseLCTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}