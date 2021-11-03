using Library.Model.Enums;
using Library.Model.SalesManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.SalesManagements
{
    public class SalesServiceConfiguration : EntityTypeConfiguration<SalesService>
    {
        public SalesServiceConfiguration()
        {
            ToTable(nameof(SalesService), DbSchema.Transaction);
            HasKey(t => t.Id);
            Property(r => r.Amount).HasPrecision(18, 10);
            Property(r => r.TaxAmount).HasPrecision(18, 10);
            Property(r => r.NetAmount).HasPrecision(18, 10);
            Ignore(r => r.ModelState);
        }
    }
}