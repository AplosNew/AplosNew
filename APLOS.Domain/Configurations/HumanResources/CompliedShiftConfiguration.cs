using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class CompliedShiftConfiguration : EntityTypeConfiguration<CompliedShift>
    {
        public CompliedShiftConfiguration()
        {
            ToTable(nameof(CompliedShift), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}