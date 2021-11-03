#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class BonusPolicyMonthlyRetainDistributionPmtConfiguration : EntityTypeConfiguration<BonusPolicyMonthlyRetainDistributionPmt>
    {
        public BonusPolicyMonthlyRetainDistributionPmtConfiguration()
        {
            HasKey(t => t.ID);
            Ignore(r => r.ModelState);
            ToTable(nameof(BonusPolicyMonthlyRetainDistributionPmt), DbSchema.Dbo);
        }
    }
}