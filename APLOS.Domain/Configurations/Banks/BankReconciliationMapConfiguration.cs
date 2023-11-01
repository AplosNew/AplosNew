using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Banks
{
    public class BankReconciliationMapConfiguration : EntityTypeConfiguration<BankReconciliationMap>
    {
        public BankReconciliationMapConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(BankReconciliationMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}