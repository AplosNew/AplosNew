#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class AttendanceGroupConfiguration : EntityTypeConfiguration<AttendanceGroup>
    {
        public AttendanceGroupConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(AttendanceGroup), DbSchema.Dbo);
        }
    }
}