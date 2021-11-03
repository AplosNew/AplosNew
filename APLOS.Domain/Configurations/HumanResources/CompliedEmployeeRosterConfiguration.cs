using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class CompliedEmployeeRosterConfiguration : EntityTypeConfiguration<CompliedEmployeeRoster>
    {
        public CompliedEmployeeRosterConfiguration()
        {
            ToTable(nameof(CompliedEmployeeRoster), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}