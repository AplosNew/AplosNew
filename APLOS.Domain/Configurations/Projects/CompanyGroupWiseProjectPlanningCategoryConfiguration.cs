using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class CompanyGroupWiseProjectPlanningCategoryConfiguration : EntityTypeConfiguration<CompanyGroupWiseProjectPlanningCategory>
    {
        public CompanyGroupWiseProjectPlanningCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(CompanyGroupWiseProjectPlanningCategory), DbSchema.HKP);
        }
    }
}