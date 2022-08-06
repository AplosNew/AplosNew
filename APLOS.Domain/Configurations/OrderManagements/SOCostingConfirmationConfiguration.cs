using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SOCostingConfirmationConfiguration : EntityTypeConfiguration<SOCostingConfirmation>
    {
        public SOCostingConfirmationConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOCostingConfirmation), DbSchema.Dbo);
        }
    }
}