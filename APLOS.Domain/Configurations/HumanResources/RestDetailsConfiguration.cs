using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class RestDetailsConfiguration : EntityTypeConfiguration<AttendanceRestDetail>
    {
        public RestDetailsConfiguration()
        {
            ToTable(nameof(AttendanceRestDetail), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}