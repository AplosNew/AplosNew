using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class OperationPositionMPBudgetConfiguration : EntityTypeConfiguration<OperationPositionMPBudget>
    {
        public OperationPositionMPBudgetConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(OperationPositionMPBudget), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}