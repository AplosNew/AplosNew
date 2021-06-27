using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class OtherInvoiceConfiguration : EntityTypeConfiguration<OtherInvoice>
    {
        public OtherInvoiceConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(OtherInvoice), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}