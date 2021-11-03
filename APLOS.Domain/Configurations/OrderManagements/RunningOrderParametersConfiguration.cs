using Library.Model.Enums;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class RunningOrderParametersConfiguration : EntityTypeConfiguration<RunningOrderParameter>
    {
        public RunningOrderParametersConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(RunningOrderParameter), DbSchema.HKP);
        }
    }
}