#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeBudgetCodeHistoryConfiguration : EntityTypeConfiguration<EmployeeBudgetCodeHistory>
    {
        public EmployeeBudgetCodeHistoryConfiguration()
        {
            ToTable(nameof(EmployeeBudgetCodeHistory), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}