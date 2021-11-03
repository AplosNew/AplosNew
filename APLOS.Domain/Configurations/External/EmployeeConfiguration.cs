#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class EmployeeConfiguration : EntityTypeConfiguration<Employee>
    {
        public EmployeeConfiguration()
        {
            ToTable(nameof(Employee), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}