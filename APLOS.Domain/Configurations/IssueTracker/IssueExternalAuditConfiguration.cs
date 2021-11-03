using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueExternalAuditConfiguration : EntityTypeConfiguration<IssueExternalAudit>
    {
        public IssueExternalAuditConfiguration()
        {
            ToTable(nameof(IssueExternalAudit), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}