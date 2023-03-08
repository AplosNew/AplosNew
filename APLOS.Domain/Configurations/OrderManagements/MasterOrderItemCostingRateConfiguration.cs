using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class MasterOrderItemCostingRateConfiguration : EntityTypeConfiguration<MasterOrderItemCostingRate>
    {
        public MasterOrderItemCostingRateConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(MasterOrderItemCostingRate), DbSchema.Dbo);
        }
    }
}