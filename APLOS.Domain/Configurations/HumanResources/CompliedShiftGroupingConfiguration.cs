using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class CompliedShiftGroupingConfiguration : EntityTypeConfiguration<CompliedShiftGrouping>
    {
        public CompliedShiftGroupingConfiguration()
        {
            ToTable(nameof(CompliedShiftGrouping), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}