#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SalaryHeadGLConfiguration : EntityTypeConfiguration<SalaryHeadGL>
    {
        public SalaryHeadGLConfiguration()
        {
            ToTable(nameof(SalaryHeadGL), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}