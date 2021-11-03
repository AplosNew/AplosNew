using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class FirstCharacteristicsConfiguration : EntityTypeConfiguration<FirstCharacteristics>
    {
        public FirstCharacteristicsConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(FirstCharacteristics), DbSchema.Transaction);
        }
    }
}