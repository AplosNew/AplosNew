#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class AdvanceReqScheduleConfiguration : EntityTypeConfiguration<AdvanceReqSchedule>
    {
        public AdvanceReqScheduleConfiguration()//
        {
            ToTable(nameof(AdvanceReqSchedule), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}