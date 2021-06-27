#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class PaidHoursEmployeeAssignConfiguration : EntityTypeConfiguration<PaidHoursEmployeeAssign>
    {
        public PaidHoursEmployeeAssignConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PaidHoursEmployeeAssign), DbSchema.Masters);
        }
    }
}