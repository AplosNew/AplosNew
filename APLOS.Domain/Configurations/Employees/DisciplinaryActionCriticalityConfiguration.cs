#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class DisciplinaryActionCriticalityConfiguration : EntityTypeConfiguration<DisciplinaryActionCriticality>
    {
        public DisciplinaryActionCriticalityConfiguration()
        {
            ToTable(nameof(DisciplinaryActionCriticality), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}