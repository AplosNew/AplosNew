#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class CompanyGroupSOPDocumentSubCategoryConfiguration : EntityTypeConfiguration<CompanyGroupSOPDocumentSubCategory>
    {
        public CompanyGroupSOPDocumentSubCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupSOPDocumentSubCategory), DbSchema.HKP);
        }
    }
}