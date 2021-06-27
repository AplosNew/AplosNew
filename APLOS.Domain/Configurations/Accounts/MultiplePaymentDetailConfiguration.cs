using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class MultiplePaymentDetailConfiguration : EntityTypeConfiguration<MultiplePaymentDetail>
    {
        public MultiplePaymentDetailConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(MultiplePaymentDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}