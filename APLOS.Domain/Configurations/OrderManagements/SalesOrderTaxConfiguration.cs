using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SalesOrderTaxConfiguration : EntityTypeConfiguration<SalesOrderTax>
    {
        public SalesOrderTaxConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SalesOrderTax), DbSchema.Transaction);
        }
    }
}