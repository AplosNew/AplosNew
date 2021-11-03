using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class SOPCategoryConfiguration : EntityTypeConfiguration<SOPCategory>
    {
        public SOPCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable("SOPCategory", DbSchema.HKP);
        }
    }
}