#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class BonusPolicyMonthlyRetainEmpWiseCalculationConfiguration : EntityTypeConfiguration<BonusPolicyMonthlyRetainEmpWiseCalculation>
    {
        public BonusPolicyMonthlyRetainEmpWiseCalculationConfiguration()
        {
            HasKey(t => t.ID);
            Ignore(r => r.ModelState);
            ToTable(nameof(BonusPolicyMonthlyRetainEmpWiseCalculation), DbSchema.Dbo);
        }
    }
}