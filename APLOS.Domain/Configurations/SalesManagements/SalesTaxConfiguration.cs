using Library.Model.Enums;
using Library.Model.SalesManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.SalesManagements
{
    public class SalesTaxConfiguration : EntityTypeConfiguration<SalesTax>
    {
        public SalesTaxConfiguration()
        {
            ToTable(nameof(SalesTax), DbSchema.Transaction);
            HasKey(t => t.Id);
            Property(r => r.Percentage).HasPrecision(18, 4);
            Property(r => r.Amount).HasPrecision(18, 2);
            Ignore(r => r.ModelState);
        }
    }
}