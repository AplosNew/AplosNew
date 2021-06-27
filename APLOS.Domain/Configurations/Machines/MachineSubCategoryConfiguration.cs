using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class MachineSubCategoryConfiguration : EntityTypeConfiguration<MachineSubCategory>
    {
        public MachineSubCategoryConfiguration()
        {
            // Table & Column Configuration
            ToTable(nameof(MachineSubCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}