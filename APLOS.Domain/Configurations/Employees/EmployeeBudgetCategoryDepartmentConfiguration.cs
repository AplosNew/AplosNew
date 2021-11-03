#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeBudgetCategoryDepartmentConfiguration : EntityTypeConfiguration<EmployeeBudgetCategoryDepartment>
    {
        public EmployeeBudgetCategoryDepartmentConfiguration()
        {
            ToTable(nameof(EmployeeBudgetCategoryDepartment), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}