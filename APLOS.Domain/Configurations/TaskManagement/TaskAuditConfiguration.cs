using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskAuditConfiguration : EntityTypeConfiguration<TaskAudit>
    {
        public TaskAuditConfiguration()
        {
            ToTable(nameof(TaskAudit), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}