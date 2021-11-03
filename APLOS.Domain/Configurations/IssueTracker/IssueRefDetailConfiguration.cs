using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueRefDetailConfiguration : EntityTypeConfiguration<IssueRefDetail>
    {
        public IssueRefDetailConfiguration()
        {
            ToTable(nameof(IssueRefDetail), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}