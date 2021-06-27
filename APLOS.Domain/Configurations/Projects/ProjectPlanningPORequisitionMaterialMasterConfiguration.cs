using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningPORequisitionMaterialMasterConfiguration : EntityTypeConfiguration<ProjectPlanningPORequisitionMaterialMaster>
    {
        public ProjectPlanningPORequisitionMaterialMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningPORequisitionMaterialMaster), DbSchema.Masters);
        }
    }
}