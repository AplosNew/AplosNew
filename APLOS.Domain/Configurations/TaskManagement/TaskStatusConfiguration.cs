using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskStatusConfiguration : EntityTypeConfiguration<TaskStatus>
    {
        public TaskStatusConfiguration()
        {
            ToTable(nameof(TaskStatus), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}