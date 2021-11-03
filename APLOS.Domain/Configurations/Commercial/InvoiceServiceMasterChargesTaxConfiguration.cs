using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Commercial
{
    public class InvoiceServiceMasterChargesTaxConfiguration : EntityTypeConfiguration<InvoiceServiceMasterChargesTax>
    {
        public InvoiceServiceMasterChargesTaxConfiguration()
        {
            ToTable(nameof(InvoiceServiceMasterChargesTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}