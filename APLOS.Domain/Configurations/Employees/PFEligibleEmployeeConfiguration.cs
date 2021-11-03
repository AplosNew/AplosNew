#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class PFEligibleEmployeeConfiguration : EntityTypeConfiguration<PFEligibleEmployee>
    {
        public PFEligibleEmployeeConfiguration()
        {
            ToTable(nameof(PFEligibleEmployee), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}