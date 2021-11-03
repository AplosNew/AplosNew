#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class EmployeeAttendanceGroupConfiguration : EntityTypeConfiguration<EmployeeAttendanceGroup>
    {
        public EmployeeAttendanceGroupConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(EmployeeAttendanceGroup), DbSchema.Dbo);
        }
    }
}