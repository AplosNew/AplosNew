#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeDependantConfiguration : EntityTypeConfiguration<EmployeeDependantInfo>
    {
        public EmployeeDependantConfiguration()
        {
            ToTable(nameof(EmployeeDependantInfo), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}