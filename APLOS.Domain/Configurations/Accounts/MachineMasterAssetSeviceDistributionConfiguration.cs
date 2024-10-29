using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class MachineMasterAssetSeviceDistributionConfiguration : EntityTypeConfiguration<MachineMasterAssetSeviceDistribution>
    {
        public MachineMasterAssetSeviceDistributionConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(MachineMasterAssetSeviceDistribution), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.GLGeneralInfoId);
            Ignore(r => r.BudgetMasterId);
            Ignore(r => r.ActivityId);
        }
    }
}