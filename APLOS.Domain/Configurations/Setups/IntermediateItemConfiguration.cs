using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class IntermediateItemConfiguration : EntityTypeConfiguration<IntermediateItem>
    {
        public IntermediateItemConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(IntermediateItem), DbSchema.HKP);
        }
    }
}