#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class CompanyGroupMachineClassConfiguration : EntityTypeConfiguration<CompanyGroupMachineClass>
    {
        public CompanyGroupMachineClassConfiguration()
        {
            ToTable(DbTable.CompanyGroupMachineClass, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}