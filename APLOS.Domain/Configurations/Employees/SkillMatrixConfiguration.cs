#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SkillMatrixConfiguration : EntityTypeConfiguration<SkillMatrix>
    {
        public SkillMatrixConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SkillMatrix), DbSchema.HKP);  
        }
    }
}