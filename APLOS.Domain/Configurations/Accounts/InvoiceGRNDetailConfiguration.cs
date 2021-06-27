using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class InvoiceGRNDetailConfiguration : EntityTypeConfiguration<InvoiceGRNDetail>
    {
        public InvoiceGRNDetailConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(InvoiceGRNDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}