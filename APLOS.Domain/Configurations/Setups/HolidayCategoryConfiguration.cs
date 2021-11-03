#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class HolidayCategoryConfiguration : EntityTypeConfiguration<HolidayCategory>
    {
        public HolidayCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(HolidayCategory), DbSchema.SystemConfigurationAndSetup);
        }
    }
}