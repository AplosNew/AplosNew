#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeProbationalPeriodConfiguration : EntityTypeConfiguration<EmployeeProbationalPeriod>
    {
        public EmployeeProbationalPeriodConfiguration()
        {
            Ignore(r => r.ApprovalStatus);
            Ignore(Doc => Doc.NewDOC);
            ToTable(nameof(EmployeeProbationalPeriod), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}