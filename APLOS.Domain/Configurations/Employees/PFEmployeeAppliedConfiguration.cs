#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class PFEmployeeAppliedConfiguration : EntityTypeConfiguration<PFEmployeeApplied>
    {
        public PFEmployeeAppliedConfiguration()
        {
            ToTable(nameof(PFEmployeeApplied), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}