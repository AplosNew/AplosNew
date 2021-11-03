using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class DispatchUnitSKUConfiguration : EntityTypeConfiguration<DispatchUnitSKU>
    {
        public DispatchUnitSKUConfiguration()
        {
            Ignore(r => r.ModelState);
            Property(t => t.Qty).HasPrecision(18, 10);
            ToTable(nameof(DispatchUnitSKU), DbSchema.Transaction);
        }
    }
}