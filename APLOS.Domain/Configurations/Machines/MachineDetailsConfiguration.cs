#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class MachineDetailsConfiguration : EntityTypeConfiguration<MachineDetails>
    {
        public MachineDetailsConfiguration()
        {
            ToTable(nameof(MachineDetails), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}