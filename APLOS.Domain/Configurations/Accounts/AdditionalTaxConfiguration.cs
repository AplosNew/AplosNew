using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class AdditionalTaxConfiguration : EntityTypeConfiguration<AdditionalTax>
    {
        public AdditionalTaxConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(AdditionalTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}