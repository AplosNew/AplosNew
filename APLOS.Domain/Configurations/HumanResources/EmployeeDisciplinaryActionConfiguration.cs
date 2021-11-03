using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class EmployeeDisciplinaryActionConfiguration : EntityTypeConfiguration<EmployeeDisciplinaryAction>
    {
        public EmployeeDisciplinaryActionConfiguration()
        {
            ToTable(nameof(EmployeeDisciplinaryAction), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}