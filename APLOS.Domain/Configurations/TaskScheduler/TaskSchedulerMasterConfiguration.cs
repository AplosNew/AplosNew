#region Using

using Library.Model.Enums;
using Library.Model.TaskScheduler;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.TaskScheduler
{
    public class TaskSchedulerMasterConfiguration : EntityTypeConfiguration<TaskSchedulerMaster>
    {
        public TaskSchedulerMasterConfiguration()
        {
            ToTable(nameof(TaskSchedulerMaster), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}