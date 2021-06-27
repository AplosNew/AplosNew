using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class PrdOrdSettingConfiguration : EntityTypeConfiguration<PrdOrdSetting>
    {
        public PrdOrdSettingConfiguration()
        {
            Ignore(r => r.ModelState);
            Ignore(t => t.ColumnAlias);
            ToTable(nameof(PrdOrdSetting), DbSchema.SystemConfigurationAndSetup);
        }
    }
}