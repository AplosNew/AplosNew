#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class HolidayAbsentismAssignmentConfiguration : EntityTypeConfiguration<HolidayAbsentismAssignment>
    {
        public HolidayAbsentismAssignmentConfiguration()
        {
            ToTable(nameof(HolidayAbsentismAssignment), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}