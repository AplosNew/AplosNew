using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class EmpDateWiseShiftAssignConfiguration : EntityTypeConfiguration<EmpDateWiseShiftAssign>
    {
        public EmpDateWiseShiftAssignConfiguration()
        {
            ToTable(nameof(EmpDateWiseShiftAssign), DbSchema.Dbo);
            Ignore(a => a.ModelState);
            HasKey(r => r.EmpSystemID);
        }
    }
}