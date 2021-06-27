using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class SalaryFixationSettingConfiguration : EntityTypeConfiguration<SalaryFixationSetting>
    {
        public SalaryFixationSettingConfiguration()
        {
            ToTable(nameof(SalaryFixationSetting), DbSchema.SystemConfigurationAndSetup);
            Ignore(r => r.ModelState);
        }
    }
}