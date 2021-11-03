using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class ProductionSystemConfiguration : EntityTypeConfiguration<ProductionSystem>
    {
        public ProductionSystemConfiguration()
        {
            ToTable(nameof(ProductionSystem), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}