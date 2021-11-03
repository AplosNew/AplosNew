using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class LegalSalaryGradeHeadConfiguration : EntityTypeConfiguration<LegalSalaryGradeHead>
    {
        public LegalSalaryGradeHeadConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LegalSalaryGradeHead), DbSchema.SystemConfigurationAndSetup);
        }
    }
}