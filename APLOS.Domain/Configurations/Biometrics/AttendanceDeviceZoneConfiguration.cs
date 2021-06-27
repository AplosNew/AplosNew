#region Using

using Library.Model.Biometrics;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Biometrics
{
    public class AttendanceDeviceZoneConfiguration : EntityTypeConfiguration<AttendanceDeviceZone>
    {
        public AttendanceDeviceZoneConfiguration()
        {
            ToTable(nameof(AttendanceDeviceZone), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}