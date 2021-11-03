using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SampleOrderSubMaterialConfiguration : EntityTypeConfiguration<SampleOrderSubMaterial>
    {
        public SampleOrderSubMaterialConfiguration()
        {
            Ignore(r => r.ModelState);
            Property(t => t.Rate).HasPrecision(18, 4);
            ToTable(nameof(SampleOrderSubMaterial), DbSchema.Transaction);
        }
    }
}