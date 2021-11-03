using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class SpecialTaxConfiguration : EntityTypeConfiguration<SpecialTax>
    {
        public SpecialTaxConfiguration()
        {
            ToTable(nameof(SpecialTax), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}