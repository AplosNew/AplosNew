using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningMaterialMasterConfiguration : EntityTypeConfiguration<ProjectPlanningMaterialMaster>
    {
        public ProjectPlanningMaterialMasterConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningMaterialMaster), DbSchema.Masters);
        }
    }
}