using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningDetailConfiguration : EntityTypeConfiguration<ProjectPlanningDetail>
    {
        public ProjectPlanningDetailConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningDetail), DbSchema.Masters);
        }
    }
}