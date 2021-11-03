using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class RestConfiguration : EntityTypeConfiguration<AttendanceRest>
    {
        public RestConfiguration()
        {
            ToTable(nameof(AttendanceRest), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}