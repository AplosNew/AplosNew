using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class MachineVariantConfiguration : EntityTypeConfiguration<MachineVariant>
    {
        public MachineVariantConfiguration()
        {
            ToTable(nameof(MachineVariant), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}