#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeLandLoardConfiguration : EntityTypeConfiguration<EmployeeLandLordInfo>
    {
        public EmployeeLandLoardConfiguration()
        {
            ToTable(nameof(EmployeeLandLordInfo), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}