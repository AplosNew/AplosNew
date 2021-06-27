using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class MachineCategoryConfiguration : EntityTypeConfiguration<MachineCategory>
    {
        public MachineCategoryConfiguration()
        {
            // Table & Column Configuration
            ToTable(nameof(MachineCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}