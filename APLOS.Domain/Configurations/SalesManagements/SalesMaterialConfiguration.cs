using Library.Model.Enums;
using Library.Model.SalesManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.SalesManagements
{
    public class SalesMaterialConfiguration : EntityTypeConfiguration<SalesMaterial>
    {
        public SalesMaterialConfiguration()
        {
            ToTable(nameof(SalesMaterial), DbSchema.Transaction);
            HasKey(t => t.Id);
            Property(r => r.BaseAmount).HasPrecision(18, 2);
            Property(r => r.BaseRate).HasPrecision(18, 4);
            Property(r => r.BooksCurrencyBaseRate).HasPrecision(18, 4);
            Property(r => r.BaseUoMFactor).HasPrecision(18, 2);
            Property(r => r.TransactionAmount).HasPrecision(18, 2);
            Property(r => r.TransactionRate).HasPrecision(18, 4);
            Property(r => r.TaxAmount).HasPrecision(18, 2);
            Property(r => r.NetAmount).HasPrecision(18, 2);
            Ignore(r => r.ModelState);
        }
    }
}