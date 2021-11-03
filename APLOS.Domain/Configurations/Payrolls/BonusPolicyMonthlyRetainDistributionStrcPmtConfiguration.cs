#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class BonusPolicyMonthlyRetainDistributionStrcPmtConfiguration : EntityTypeConfiguration<BonusPolicyMonthlyRetainDistributionStrcPmt>
    {
        public BonusPolicyMonthlyRetainDistributionStrcPmtConfiguration()
        {
            HasKey(t => t.ID);
            Ignore(r => r.ModelState);
            ToTable(nameof(BonusPolicyMonthlyRetainDistributionStrcPmt), DbSchema.Dbo);
        }
    }
}