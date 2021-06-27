using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningPurchaseOrderConfiguration : EntityTypeConfiguration<ProjectPlanningPurchaseOrder>
    {
        public ProjectPlanningPurchaseOrderConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningPurchaseOrder), DbSchema.Masters);
        }
    }
}