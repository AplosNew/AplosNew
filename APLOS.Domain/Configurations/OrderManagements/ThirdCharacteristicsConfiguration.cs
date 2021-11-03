using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ThirdCharacteristicsConfiguration : EntityTypeConfiguration<ThirdCharacteristics>
    {
        public ThirdCharacteristicsConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ThirdCharacteristics), DbSchema.Transaction);
        }
    }
}