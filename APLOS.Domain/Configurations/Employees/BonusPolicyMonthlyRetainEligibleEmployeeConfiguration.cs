#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class BonusPolicyMonthlyRetainEligibleEmployeeConfiguration : EntityTypeConfiguration<BonusPolicyMonthlyRetainEligibleEmployee>
    {
        public BonusPolicyMonthlyRetainEligibleEmployeeConfiguration()
        {
            ToTable(nameof(BonusPolicyMonthlyRetainEligibleEmployee), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}