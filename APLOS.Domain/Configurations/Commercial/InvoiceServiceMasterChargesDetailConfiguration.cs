using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Commercial
{
    public class InvoiceServiceMasterChargesDetailConfiguration : EntityTypeConfiguration<InvoiceServiceMasterChargesDetail>
    {
        public InvoiceServiceMasterChargesDetailConfiguration()
        {
            ToTable(nameof(InvoiceServiceMasterChargesDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}