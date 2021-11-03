using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskNotificationConfiguration : EntityTypeConfiguration<TaskNotification>
    {
        public TaskNotificationConfiguration()
        {
            ToTable(nameof(TaskNotification), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}