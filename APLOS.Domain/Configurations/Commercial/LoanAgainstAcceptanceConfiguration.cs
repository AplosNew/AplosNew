using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Commercial
{
    public class LoanAgainstAcceptanceConfiguration : EntityTypeConfiguration<LoanAgainstAcceptance>
    {
        public LoanAgainstAcceptanceConfiguration()
        {
            ToTable(nameof(LoanAgainstAcceptance), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}