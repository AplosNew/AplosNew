using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AccessControllerEmployeeTagConfiguration : EntityTypeConfiguration<AccessControllerEmployeeTag>
    {
        public AccessControllerEmployeeTagConfiguration()
        {
            ToTable(nameof(AccessControllerEmployeeTag), DbSchema.Dbo);
            Ignore(r => r.ModelState);
            Ignore(r => r.DeviceIP);
        }
    }
}