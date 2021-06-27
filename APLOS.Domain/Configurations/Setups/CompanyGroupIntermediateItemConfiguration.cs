using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Setups
{
    public class CompanyGroupIntermediateItemConfiguration : EntityTypeConfiguration<CompanyGroupIntermediateItem>
    {
        public CompanyGroupIntermediateItemConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupIntermediateItem), DbSchema.HKP);
        }
    }
}