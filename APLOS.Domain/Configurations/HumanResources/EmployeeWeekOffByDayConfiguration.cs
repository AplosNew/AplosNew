using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class EmployeeWeekOffByDayConfiguration : EntityTypeConfiguration<EmployeeWeekOffByDay>
    {
        public EmployeeWeekOffByDayConfiguration()
        {
            ToTable(nameof(EmployeeWeekOffByDay), DbSchema.Dbo);
            Ignore(a => a.ModelState);
            HasKey(r => r.SystemID);
        }
    }
}