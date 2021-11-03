#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class LeaveOpeningBalanceConfiguration : EntityTypeConfiguration<LeaveOpeningBalance>
    {
        public LeaveOpeningBalanceConfiguration()
        {
            ToTable(nameof(LeaveOpeningBalance), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}