#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class CompanyEmpConfiguration : EntityTypeConfiguration<CompanyEmp>
    {
        public CompanyEmpConfiguration()
        {
            ToTable("Company", DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}