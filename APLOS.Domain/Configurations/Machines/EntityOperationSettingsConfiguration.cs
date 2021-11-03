#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class EntityOperationSettingsConfiguration : EntityTypeConfiguration<EntityOperationSettings>
    {
        public EntityOperationSettingsConfiguration()
        {
            ToTable(nameof(EntityOperationSettings), DbSchema.SystemConfigurationAndSetup);
            Ignore(r => r.ModelState);
        }
    }
}