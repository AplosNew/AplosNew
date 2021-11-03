using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskTypeConfiguration : EntityTypeConfiguration<TaskType>
    {
        public TaskTypeConfiguration()
        {
            ToTable(nameof(TaskType), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}