using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class EmployeeShiftAssignConfiguration : EntityTypeConfiguration<EmployeeShiftAssign>
    {
        public EmployeeShiftAssignConfiguration()
        {
            ToTable(nameof(EmployeeShiftAssign), DbSchema.Dbo);
            Ignore(a => a.ModelState);
            HasKey(r => r.SystemID);
        }
    }
}