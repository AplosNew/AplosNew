using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class CompliedShiftGroupDetailConfiguration : EntityTypeConfiguration<CompliedShiftGroupDetail>
    {
        public CompliedShiftGroupDetailConfiguration()
        {
            ToTable(nameof(CompliedShiftGroupDetail), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}