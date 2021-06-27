#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class DesignationMasterConfigurationConfiguration : EntityTypeConfiguration<DesignationMasterConfiguration>
    {
        public DesignationMasterConfigurationConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(DesignationMasterConfiguration), DbSchema.SystemConfigurationAndSetup);
        }
    }
}