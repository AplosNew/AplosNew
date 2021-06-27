using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Commercial
{
    public class InvoiceServiceMasterChargesConfiguration : EntityTypeConfiguration<InvoiceServiceMasterCharges>
    {
        public InvoiceServiceMasterChargesConfiguration()
        {
            ToTable(nameof(InvoiceServiceMasterCharges), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}