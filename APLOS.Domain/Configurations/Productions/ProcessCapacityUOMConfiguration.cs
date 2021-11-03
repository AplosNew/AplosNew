using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class ProcessCapacityUOMConfiguration : EntityTypeConfiguration<ProcessCapacityUOM>
    {
        public ProcessCapacityUOMConfiguration()
        {
            Ignore(r => r.ModelState);
            Ignore(t => t.Archive);
            ToTable(nameof(ProcessCapacityUOM), DbSchema.SystemConfigurationAndSetup);
        }
    }
}