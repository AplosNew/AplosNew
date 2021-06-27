using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class MasterOrderResPersonConfiguration : EntityTypeConfiguration<MasterOrderResPerson>
    {
        public MasterOrderResPersonConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(MasterOrderResPerson), DbSchema.Transaction);
        }
    }
}