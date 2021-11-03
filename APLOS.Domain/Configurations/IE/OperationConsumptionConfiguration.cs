using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class OperationConsumptionConfiguration : EntityTypeConfiguration<OperationConsumption>
    {
        public OperationConsumptionConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(OperationConsumption), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}