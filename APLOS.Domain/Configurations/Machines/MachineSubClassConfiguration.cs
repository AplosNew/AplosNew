#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class MachineSubClassConfiguration : EntityTypeConfiguration<MachineSubClass>
    {
        public MachineSubClassConfiguration()
        {
            ToTable(nameof(MachineSubClass), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}