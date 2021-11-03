using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class AdditionalTaxDetailConfiguration : EntityTypeConfiguration<AdditionalTaxDetail>
    {
        public AdditionalTaxDetailConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(AdditionalTaxDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}