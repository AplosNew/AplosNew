#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class PreRecruitmentEmployeeConfiguration : EntityTypeConfiguration<PreRecruitmentEmployee>
    {
        public PreRecruitmentEmployeeConfiguration()
        {
            Ignore(r => r.Active);
            Ignore(r => r.EmployeeId);
            Ignore(r => r.EmployeeCode);
            ToTable(nameof(PreRecruitmentEmployee), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}