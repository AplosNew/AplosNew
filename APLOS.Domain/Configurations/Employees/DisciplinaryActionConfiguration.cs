#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class DisciplinaryActionConfiguration : EntityTypeConfiguration<DisciplinaryAction>
    {
        public DisciplinaryActionConfiguration()
        {
            ToTable(nameof(DisciplinaryAction), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}