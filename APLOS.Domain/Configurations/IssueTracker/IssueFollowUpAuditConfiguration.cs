using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueFollowUpResponsibleConfiguration : EntityTypeConfiguration<IssueFollowUpAudit>
    {
        public IssueFollowUpResponsibleConfiguration()
        {
            ToTable(nameof(IssueFollowUpAudit), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}