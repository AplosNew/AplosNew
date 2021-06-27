#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmployeeDocumentConfiguration : EntityTypeConfiguration<EmployeeDocument>
    {
        public EmployeeDocumentConfiguration()
        {
            ToTable(nameof(EmployeeDocument), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}