using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class PlantSettingConfiguration : EntityTypeConfiguration<PlantSetting>
    {
        public PlantSettingConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PlantSetting), DbSchema.SystemConfigurationAndSetup);
        }
    }
}