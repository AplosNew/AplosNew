#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class WeeklyAbsentismAssignmentConfiguration : EntityTypeConfiguration<WeeklyAbsentismAssignment>
    {
        public WeeklyAbsentismAssignmentConfiguration()
        {
            ToTable(nameof(WeeklyAbsentismAssignment), DbSchema.SystemConfigurationAndSetup);
            Ignore(r => r.ModelState);
        }
    }
}