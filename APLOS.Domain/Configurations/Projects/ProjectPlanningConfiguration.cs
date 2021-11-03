using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningConfiguration : EntityTypeConfiguration<ProjectPlanning>
    {
        public ProjectPlanningConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProjectPlanning), DbSchema.Masters);
        }
    }
}