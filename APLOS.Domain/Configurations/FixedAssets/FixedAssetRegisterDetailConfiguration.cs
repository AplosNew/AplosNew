using Library.Model.Enums;
using Library.Model.FixedAsset;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class FixedAssetRegisterDetailConfiguration : EntityTypeConfiguration<FixedAssetRegisterDetail>
    {
        public FixedAssetRegisterDetailConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(FixedAssetRegisterDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}