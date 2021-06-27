using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueImportanceConfiguration : EntityTypeConfiguration<IssueImportance>
    {
        public IssueImportanceConfiguration()
        {
            ToTable(nameof(IssueImportance), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}