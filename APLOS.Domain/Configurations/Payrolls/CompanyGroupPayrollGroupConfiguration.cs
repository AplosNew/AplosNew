#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class PayrollGroupConfiguration : EntityTypeConfiguration<PayrollGroup>
    {
        public PayrollGroupConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PayrollGroup), DbSchema.HKP);
        }
    }
}