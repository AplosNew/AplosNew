#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeLeaveSummaryConfiguration : EntityTypeConfiguration<EmployeeLeaveSummary>
    {
        public EmployeeLeaveSummaryConfiguration()
        {
            ToTable(nameof(EmployeeLeaveSummary), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}