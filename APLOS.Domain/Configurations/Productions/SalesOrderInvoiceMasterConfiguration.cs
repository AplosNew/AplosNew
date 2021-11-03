using Library.Model.Enums;
using Library.Model.Productions.SalesOrderInvoice;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class SalesOrderInvoiceMasterConfiguration : EntityTypeConfiguration<SalesOrderInvoiceMaster>
    {
        public SalesOrderInvoiceMasterConfiguration()
        {
            // Primary Key
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            Property(r => r.InvoiceValue).HasPrecision(18, 3);
            // Table & Column Configuration
            ToTable(nameof(SalesOrderInvoiceMaster), DbSchema.Transaction);
        }
    }
}