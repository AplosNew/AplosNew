using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class SOPSubCategoryConfiguration : EntityTypeConfiguration<SOPSubCategory>
    {
        public SOPSubCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPSubCategory), DbSchema.HKP);
        }
    }
}