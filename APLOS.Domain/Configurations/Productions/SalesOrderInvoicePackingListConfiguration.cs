using Library.Model.Enums;
using Library.Model.Productions.SalesOrderInvoice;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class SalesOrderInvoicePackingListConfiguration : EntityTypeConfiguration<SalesOrderInvoicePackingList>
    {
        public SalesOrderInvoicePackingListConfiguration()
        {
            // Primary Key
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            // this.Ignore(t => t.Rate);
            // Table & Column Configuration
            ToTable(nameof(SalesOrderInvoicePackingList), DbSchema.Transaction);
        }
    }
}