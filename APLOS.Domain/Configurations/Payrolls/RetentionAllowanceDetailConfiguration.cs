using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class RetentionAllowanceDetailConfiguration : EntityTypeConfiguration<RetentionAllowanceDetail>
    {
        public RetentionAllowanceDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(RetentionAllowanceDetail), DbSchema.SystemConfigurationAndSetup);
        }
    }
}