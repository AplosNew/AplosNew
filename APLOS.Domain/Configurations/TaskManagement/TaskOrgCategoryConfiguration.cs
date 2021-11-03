using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class TaskOrgCategoryConfiguration : EntityTypeConfiguration<TaskOrgCategory>
    {
        public TaskOrgCategoryConfiguration()
        {
            ToTable(nameof(TaskOrgCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}