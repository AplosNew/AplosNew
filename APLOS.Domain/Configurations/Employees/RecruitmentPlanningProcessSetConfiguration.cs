using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class RecruitmentPlanningProcessSetConfiguration : EntityTypeConfiguration<RecruitmentPlanningProcessSet>
    {
        public RecruitmentPlanningProcessSetConfiguration()
        {
            ToTable(nameof(RecruitmentPlanningProcessSet), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}