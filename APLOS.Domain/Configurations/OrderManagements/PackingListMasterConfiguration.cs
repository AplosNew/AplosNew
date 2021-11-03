using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class PackingListMasterConfiguration : EntityTypeConfiguration<PackingListMaster>
    {
        public PackingListMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PackingListMaster), DbSchema.Transaction);
        }
    }
}