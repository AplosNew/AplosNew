using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskManagerMasterConfiguration : EntityTypeConfiguration<TaskManagerMaster>
    {
        public TaskManagerMasterConfiguration()
        {
            ToTable(nameof(TaskManagerMaster), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}