#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SOPActivityKPIConfiguration : EntityTypeConfiguration<SOPActivityKPI>
    {
        public SOPActivityKPIConfiguration()
        {
            ToTable(nameof(SOPActivityKPI), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}