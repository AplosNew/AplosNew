using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class MasterOrderItemConfiguration : EntityTypeConfiguration<MasterOrderItem>
    {
        public MasterOrderItemConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(MasterOrderItem), DbSchema.Transaction);
        }
    }
}