using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class LineEmployeeAssignConfiguration : EntityTypeConfiguration<LineEmployeeAssign>
    {
        public LineEmployeeAssignConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LineEmployeeAssign), DbSchema.Masters);
        }
    }
}