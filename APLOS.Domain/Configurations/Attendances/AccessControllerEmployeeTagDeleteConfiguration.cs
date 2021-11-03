using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AccessControllerEmployeeTagDeleteConfiguration : EntityTypeConfiguration<AccessControllerEmployeeTagDelete>
    {
        public AccessControllerEmployeeTagDeleteConfiguration()
        {
            ToTable(nameof(AccessControllerEmployeeTagDelete), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}