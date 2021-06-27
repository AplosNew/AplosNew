using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class LegalSalaryStructureConfiguration : EntityTypeConfiguration<LegalSalaryStructure>
    {
        public LegalSalaryStructureConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LegalSalaryStructure), DbSchema.Masters);
        }
    }
}