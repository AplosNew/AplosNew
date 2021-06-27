using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class PlantConfigConfiguration : EntityTypeConfiguration<PlantConfig>
    {
        public PlantConfigConfiguration()
        {
            ToTable(nameof(PlantConfig), DbSchema.SystemConfigurationAndSetup);
            Ignore(r => r.ModelState);
        }
    }
}