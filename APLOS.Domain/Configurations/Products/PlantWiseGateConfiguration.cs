using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class PlantWiseGateConfiguration : EntityTypeConfiguration<PlantWiseGate>
    {
        public PlantWiseGateConfiguration()
        {
            // Primary Key
            ToTable(nameof(PlantWiseGate), DbSchema.Dbo);
            Ignore(r => r.ModelState);

            // Primary Key
            HasKey(t => t.Id);

        }
    }
}