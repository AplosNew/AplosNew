using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class SOPDocumentCategoryConfiguration : EntityTypeConfiguration<SOPDocumentCategory>
    {
        public SOPDocumentCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPDocumentCategory), DbSchema.HKP);
        }
    }
}