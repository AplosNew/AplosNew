using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class SalaryFixationConfiguration : EntityTypeConfiguration<SalaryFixation>
    {
        public SalaryFixationConfiguration()
        {
            ToTable(nameof(SalaryFixation), DbSchema.SystemConfigurationAndSetup);
            Ignore(r => r.ModelState);
            Ignore(t => t.SalFixSetId);
            Ignore(t => t.FixationStatusL);
            Ignore(t => t.FixationStatusN);
        }
    }
}