using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskClassConfiguration : EntityTypeConfiguration<TaskClass>
    {
        public TaskClassConfiguration()
        {
            ToTable(nameof(TaskClass), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}