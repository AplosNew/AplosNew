using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class ItemScanChildDataConfiguration : EntityTypeConfiguration<ItemScanChild>
    {
        public ItemScanChildDataConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(ItemScanChild), DbSchema.Dbo);
        }
    }
}