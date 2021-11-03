using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskManagerSubTasksConfiguration : EntityTypeConfiguration<TaskManagerSubTasks>
    {
        public TaskManagerSubTasksConfiguration()
        {
            ToTable(nameof(TaskManagerSubTasks), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}