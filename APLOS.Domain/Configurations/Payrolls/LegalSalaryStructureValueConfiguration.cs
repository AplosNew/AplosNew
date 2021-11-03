using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class LegalSalaryStructureValueConfiguration : EntityTypeConfiguration<LegalSalaryStructureValue>
    {
        public LegalSalaryStructureValueConfiguration()
        {
            ToTable(nameof(LegalSalaryStructureValue), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}