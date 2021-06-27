using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class CompanyGroupSOPCategoryConfiguration : EntityTypeConfiguration<CompanyGroupSOPCategory>
    {
        public CompanyGroupSOPCategoryConfiguration()
        {
            ToTable(nameof(CompanyGroupSOPCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}