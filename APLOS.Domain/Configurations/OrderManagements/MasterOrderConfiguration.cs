using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class MasterOrderConfiguration : EntityTypeConfiguration<MasterOrder>
    {
        public MasterOrderConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(MasterOrder), DbSchema.Transaction);
        }
    }
}