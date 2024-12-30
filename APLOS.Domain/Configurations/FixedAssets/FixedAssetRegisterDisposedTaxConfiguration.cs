using Library.Model.Enums;
using Library.Model.FixedAsset;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.FixedAssets
{
    public class FixedAssetRegisterDisposedTaxConfiguration : EntityTypeConfiguration<FixedAssetRegisterDisposedTax>
    {
        public FixedAssetRegisterDisposedTaxConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(FixedAssetRegisterDisposedTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}