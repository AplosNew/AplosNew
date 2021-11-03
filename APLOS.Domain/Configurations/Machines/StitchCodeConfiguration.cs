using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class StitchCodeConfiguration : EntityTypeConfiguration<StitchCode>
    {
        public StitchCodeConfiguration()
        {
            ToTable(nameof(StitchCode), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}