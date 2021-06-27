using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueInternalAuditConfiguration : EntityTypeConfiguration<IssueInternalAudit>
    {
        public IssueInternalAuditConfiguration()
        {
            ToTable(nameof(IssueInternalAudit), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}