using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class BiometricDeviceAsShortLeaveConfiguration : EntityTypeConfiguration<BiometricDeviceAsShortLeave>
    {
        public BiometricDeviceAsShortLeaveConfiguration()
        {
            ToTable("BiometricDeviceAsShortLV", DbSchema.Dbo);
            HasKey(t => t.SystemID);
            Ignore(r => r.ModelState);
        }
    }
}