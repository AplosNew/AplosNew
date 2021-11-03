using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskRevisionCategoryConfiguration : EntityTypeConfiguration<TaskRevisionCategory>
    {
        public TaskRevisionCategoryConfiguration()
        {
            ToTable(nameof(TaskRevisionCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}