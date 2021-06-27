using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningMachineTypeConfiguration : EntityTypeConfiguration<ProjectPlanningMachineType>
    {
        public ProjectPlanningMachineTypeConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProjectPlanningMachineType), DbSchema.Masters);
        }
    }
}