#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class ShiftGroupConfiguration : EntityTypeConfiguration<ShiftGroup>
    {
        public ShiftGroupConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ShiftGroup), DbSchema.SystemConfigurationAndSetup);
        }
    }
}