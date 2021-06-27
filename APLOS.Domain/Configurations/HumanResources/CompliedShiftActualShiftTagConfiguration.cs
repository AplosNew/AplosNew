using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class CompliedShiftActualShiftTagConfiguration : EntityTypeConfiguration<CompliedShiftAssignment>
    {
        public CompliedShiftActualShiftTagConfiguration()
        {
            ToTable(nameof(CompliedShiftAssignment), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}