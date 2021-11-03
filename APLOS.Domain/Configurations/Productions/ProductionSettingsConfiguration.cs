using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class ProductionSettingsConfiguration : EntityTypeConfiguration<ProductionSettings>
    {
        public ProductionSettingsConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductionSettings), DbSchema.Transaction);
        }
    }
}