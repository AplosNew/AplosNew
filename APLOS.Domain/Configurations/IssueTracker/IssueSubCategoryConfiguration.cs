using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IssueTracker
{
    public class IssueSubCategoryConfiguration : EntityTypeConfiguration<IssueSubCategory>
    {
        public IssueSubCategoryConfiguration()
        {
            ToTable(nameof(IssueSubCategory), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}