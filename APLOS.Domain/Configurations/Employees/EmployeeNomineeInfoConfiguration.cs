#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeNomineeInfoConfiguration : EntityTypeConfiguration<EmployeeNomineeInfo>
    {
        public EmployeeNomineeInfoConfiguration()
        {
            ToTable(nameof(EmployeeNomineeInfo), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}