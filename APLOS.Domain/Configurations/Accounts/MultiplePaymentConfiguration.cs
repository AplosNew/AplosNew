using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class MultiplePaymentConfiguration : EntityTypeConfiguration<MultiplePayment>
    {
        public MultiplePaymentConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(MultiplePayment), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}