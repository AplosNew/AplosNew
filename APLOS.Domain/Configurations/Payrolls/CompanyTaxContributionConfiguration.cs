#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class CompanyTaxContributionConfiguration : EntityTypeConfiguration<CompanyTaxContribution>
    {
        public CompanyTaxContributionConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyTaxContribution), DbSchema.Masters);
        }
    }
}