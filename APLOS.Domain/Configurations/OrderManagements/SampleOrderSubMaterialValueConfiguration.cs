using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SampleOrderSubMaterialValueConfiguration : EntityTypeConfiguration<SampleOrderSubMaterialValue>
    {
        public SampleOrderSubMaterialValueConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SampleOrderSubMaterialValue), DbSchema.Transaction);
        }
    }
}