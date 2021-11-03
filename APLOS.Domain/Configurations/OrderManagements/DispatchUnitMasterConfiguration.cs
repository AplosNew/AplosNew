using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class DispatchUnitMasterConfiguration : EntityTypeConfiguration<DispatchUnitMaster>
    {
        public DispatchUnitMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            Property(t => t.Qty).HasPrecision(18, 10);
            ToTable(nameof(DispatchUnitMaster), DbSchema.Transaction);
        }
    }
}