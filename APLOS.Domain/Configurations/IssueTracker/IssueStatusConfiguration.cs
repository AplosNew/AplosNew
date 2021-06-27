using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueStatusConfiguration : EntityTypeConfiguration<IssueStatus>
    {
        public IssueStatusConfiguration()
        {
            ToTable(nameof(IssueStatus), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}