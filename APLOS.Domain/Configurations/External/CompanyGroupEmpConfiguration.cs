#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class CompanyGroupEmpConfiguration : EntityTypeConfiguration<CompanyGroupEmp>
    {
        public CompanyGroupEmpConfiguration()
        {
            ToTable("CompanyGroup", DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}