#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class AnnualNonCashConfiguration : EntityTypeConfiguration<AnnualNonCash>
    {
        public AnnualNonCashConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(AnnualNonCash), DbSchema.SystemConfigurationAndSetup);
        }
    }
}