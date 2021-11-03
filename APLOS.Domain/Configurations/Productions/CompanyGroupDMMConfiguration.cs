#region

using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

#endregion

namespace Library.Model.Configurations.Productions
{
    public class CompanyGroupDMMConfiguration : EntityTypeConfiguration<CompanyGroupDMM>
    {
        public CompanyGroupDMMConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(CompanyGroupDMM), DbSchema.HKP);
        }
    }
}