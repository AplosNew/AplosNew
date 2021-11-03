using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class SOPDocumentSubCategoryConfiguration : EntityTypeConfiguration<SOPDocumentSubCategory>
    {
        public SOPDocumentSubCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPDocumentSubCategory), DbSchema.HKP);
        }
    }
}