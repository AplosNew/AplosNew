using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class SalaryFixationSettingDetailsConfiguration : EntityTypeConfiguration<SalaryFixationSettingDetails>
    {
        public SalaryFixationSettingDetailsConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SalaryFixationSettingDetails), DbSchema.SystemConfigurationAndSetup);
        }
    }
}