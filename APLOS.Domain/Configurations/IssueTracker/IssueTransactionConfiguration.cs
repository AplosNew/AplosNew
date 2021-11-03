using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity;
using Library.Model.IssueTracker;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueTransactionConfiguration : EntityTypeConfiguration<IssueTransaction>
    {
        public IssueTransactionConfiguration()
        {
            ToTable(nameof(IssueTransaction), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}