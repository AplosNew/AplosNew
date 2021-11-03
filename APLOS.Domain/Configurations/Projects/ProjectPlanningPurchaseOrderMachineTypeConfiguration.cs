using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningPurchaseOrderMachineTypeConfiguration : EntityTypeConfiguration<ProjectPlanningPurchaseOrderMachineType>
    {
        public ProjectPlanningPurchaseOrderMachineTypeConfiguration()
        {
            ToTable(nameof(ProjectPlanningPurchaseOrderMachineType), DbSchema.Masters);
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
        }
    }
}