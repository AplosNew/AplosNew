using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskFrequencyConfiguration : EntityTypeConfiguration<TaskFrequency>
    {
        public TaskFrequencyConfiguration()
        {
            ToTable(nameof(TaskFrequency), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}