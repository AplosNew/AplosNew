#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class FabricRollMasterIncrementValueConfiguration : EntityTypeConfiguration<FabricRollMasterIncrementValue>
    {
        public FabricRollMasterIncrementValueConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            ToTable(nameof(FabricRollMasterIncrementValue), DbSchema.SystemConfigurationAndSetup);
        }
    }
}