using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class LegalSalaryGradeConfiguration : EntityTypeConfiguration<LegalSalaryGrade>
    {
        public LegalSalaryGradeConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LegalSalaryGrade), DbSchema.SystemConfigurationAndSetup);
        }
    }
}