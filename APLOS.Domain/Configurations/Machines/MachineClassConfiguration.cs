#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class MachineClassConfiguration : EntityTypeConfiguration<MachineClass>
    {
        public MachineClassConfiguration()
        {
            ToTable(DbTable.MachineClass, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}