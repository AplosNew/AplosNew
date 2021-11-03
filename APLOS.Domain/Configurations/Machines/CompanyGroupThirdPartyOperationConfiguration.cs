#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class CompanyGroupThirdPartyOperationConfiguration : EntityTypeConfiguration<CompanyGroupThirdPartyOperation>
    {
        public CompanyGroupThirdPartyOperationConfiguration()
        {
            ToTable(nameof(CompanyGroupThirdPartyOperation), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}