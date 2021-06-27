using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningPurchaseOrderMaterialMasterConfiguration : EntityTypeConfiguration<ProjectPlanningPurchaseOrderMaterialMaster>
    {
        public ProjectPlanningPurchaseOrderMaterialMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProjectPlanningPurchaseOrderMaterialMaster), DbSchema.Masters);
        }
    }
}