using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskMasterConfiguration : EntityTypeConfiguration<TaskMaster>
    {
        public TaskMasterConfiguration()
        {
            ToTable(nameof(TaskMaster), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}