#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeIdCardIssueConfiguration : EntityTypeConfiguration<EmployeeIdCardIssue>
    {
        public EmployeeIdCardIssueConfiguration()
        {
            ToTable(nameof(EmployeeIdCardIssue), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}