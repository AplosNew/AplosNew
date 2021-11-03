using Library.Model.Enums;
using Library.Model.Productions.SalesOrderInvoice;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class SalesOrderInvoiceDetailConfiguration : EntityTypeConfiguration<SalesOrderInvoiceDetail>
    {
        public SalesOrderInvoiceDetailConfiguration()
        {
            // Primary Key
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            Property(r => r.Rate).HasPrecision(18, 3);
            // Table & Column Configuration
            ToTable(nameof(SalesOrderInvoiceDetail), DbSchema.Transaction);
        }
    }
}