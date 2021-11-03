#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class EmployeeProfileFromExcelConfiguration : EntityTypeConfiguration<EmployeeProfileFromExcel>
    {
        public EmployeeProfileFromExcelConfiguration()
        {
            ToTable(nameof(EmployeeProfileFromExcel), DbSchema.Dbo);
            HasKey(t => t.SystemId);
            Ignore(r => r.ModelState);
        }
    }
}