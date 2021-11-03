using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningRequisitionConfiguration : EntityTypeConfiguration<ProjectPlanningRequisition>
    {
        public ProjectPlanningRequisitionConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningRequisition), DbSchema.Masters);
        }
    }
}