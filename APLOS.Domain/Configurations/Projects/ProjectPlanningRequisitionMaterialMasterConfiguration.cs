using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningRequisitionMaterialMasterConfiguration : EntityTypeConfiguration<ProjectPlanningRequisitionMaterialMaster>
    {
        public ProjectPlanningRequisitionMaterialMasterConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningRequisitionMaterialMaster), DbSchema.Masters);
        }
    }
}