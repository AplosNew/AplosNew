using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity;
using Library.Model.IssueTracker;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueStandardConfiguration : EntityTypeConfiguration<IssueStandard>
    {
        public IssueStandardConfiguration()
        {
            ToTable(nameof(IssueStandard), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}