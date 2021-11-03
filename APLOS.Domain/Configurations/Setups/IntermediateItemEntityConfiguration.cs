using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class IntermediateItemEntityConfiguration : EntityTypeConfiguration<IntermediateItemEntity>
    {
        public IntermediateItemEntityConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(IntermediateItemEntity), DbSchema.HKP);
        }
    }
}