using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningCategoryConfiguration : EntityTypeConfiguration<ProjectPlanningCategory>
    {
        public ProjectPlanningCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProjectPlanningCategory), DbSchema.HKP);
        }
    }
}