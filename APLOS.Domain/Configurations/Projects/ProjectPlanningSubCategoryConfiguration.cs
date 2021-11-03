using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningSubCategoryConfiguration : EntityTypeConfiguration<ProjectPlanningSubCategory>
    {
        public ProjectPlanningSubCategoryConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningSubCategory), DbSchema.HKP);
        }
    }
}