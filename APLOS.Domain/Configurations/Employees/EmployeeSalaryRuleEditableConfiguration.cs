#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeSalaryRuleEditableConfiguration : EntityTypeConfiguration<EmployeeSalaryRuleEditable>
    {
        public EmployeeSalaryRuleEditableConfiguration()
        {
            ToTable(nameof(EmployeeSalaryRuleEditable), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}