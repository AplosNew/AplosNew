using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueSubTaskConfiguration : EntityTypeConfiguration<IssueSubTask>
    {
        public IssueSubTaskConfiguration()
        {
            ToTable(nameof(IssueSubTask), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}