using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;
using Library.Model.IssueTracker;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueTransactionDocumentsConfiguration : EntityTypeConfiguration<IssueTransactionDocuments>
    {
        public IssueTransactionDocumentsConfiguration()
        {
            ToTable(nameof(IssueTransactionDocuments), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}