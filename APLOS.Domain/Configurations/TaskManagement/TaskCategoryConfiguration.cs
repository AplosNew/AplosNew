using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskCategoryConfiguration : EntityTypeConfiguration<TaskCategory>
    {
        public TaskCategoryConfiguration()
        {
            ToTable(nameof(TaskCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}