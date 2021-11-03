using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class MasterOrderTNAConfiguration : EntityTypeConfiguration<MasterOrderTNA>
    {
        public MasterOrderTNAConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(Critical), DbSchema.Dbo);
        }
    }
}
