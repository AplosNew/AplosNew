#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class ShiftGroupDetailConfiguration : EntityTypeConfiguration<ShiftGroupDetail>
    {
        public ShiftGroupDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ShiftGroupDetail), DbSchema.SystemConfigurationAndSetup);
        }
    }
}