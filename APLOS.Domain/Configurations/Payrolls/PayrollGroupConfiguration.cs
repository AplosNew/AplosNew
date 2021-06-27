#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class CompanyGroupPayrollGroupConfiguration : EntityTypeConfiguration<CompanyGroupPayrollGroup>
    {
        public CompanyGroupPayrollGroupConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupPayrollGroup), DbSchema.HKP);
        }
    }
}