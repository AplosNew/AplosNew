using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueCategoryConfiguration : EntityTypeConfiguration<IssueCategory>
    {
        public IssueCategoryConfiguration()
        {
            ToTable(nameof(IssueCategory), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}