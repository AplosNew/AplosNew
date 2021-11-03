#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    internal class EmployeeBankInformationConfiguration : EntityTypeConfiguration<EmployeeBankInformation>
    {
        public EmployeeBankInformationConfiguration()
        {
            ToTable("EmployeeBankInfo", DbSchema.Dbo);
            HasKey(t => t.RowID);
            Ignore(r => r.ModelState);
        }
    }
}