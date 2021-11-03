using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningPurchaseOrderDetailConfiguration : EntityTypeConfiguration<ProjectPlanningPurchaseOrderDetail>
    {
        public ProjectPlanningPurchaseOrderDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProjectPlanningPurchaseOrderDetail), DbSchema.Masters);
        }
    }
}