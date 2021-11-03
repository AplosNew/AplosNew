#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class DirectTaxPaymentHeadConfiguration : EntityTypeConfiguration<DirectTaxPaymentHead>
    {
        public DirectTaxPaymentHeadConfiguration()
        {
            ToTable(nameof(DirectTaxPaymentHead), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}