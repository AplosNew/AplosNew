#region

using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

#endregion

namespace Library.Model.Configurations.Productions
{
    public class CompanyGroupProductionStatusConfiguration : EntityTypeConfiguration<CompanyGroupProductionStatus>
    {
        public CompanyGroupProductionStatusConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupProductionStatus), DbSchema.HKP);
        }
    }
}