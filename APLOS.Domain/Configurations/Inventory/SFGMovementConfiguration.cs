using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class SFGMovementConfiguration : EntityTypeConfiguration<SFGMovement>
    {
        public SFGMovementConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(SFGMovement), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}